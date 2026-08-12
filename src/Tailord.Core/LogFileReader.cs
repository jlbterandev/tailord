using System.Runtime.CompilerServices;
using System.Text;

namespace Tailord.Core;

public sealed class LogFileReader
{
    private const int BufferSize = 4096;
    private const int FileComparisonSampleSize = 256;

    public async IAsyncEnumerable<string> ReadExistingAsync(
        string path,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        await using FileStream stream = OpenFile(path);
        using StreamReader reader = CreateReader(stream);

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            yield return line;
        }
    }

    public async IAsyncEnumerable<string> FollowAsync(
        string path,
        TimeSpan pollingInterval,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        if (pollingInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(pollingInterval));
        }

        char[] characters = new char[BufferSize];

        while (true)
        {
            await using FileStream stream = OpenFile(path);
            using StreamReader reader = CreateReader(stream);
            LogLineBuffer lineBuffer = new();
            bool fileWasReplaced = false;

            while (!fileWasReplaced)
            {
                int charactersRead = await reader.ReadAsync(characters, cancellationToken);

                if (charactersRead == 0)
                {
                    if (stream.Length < stream.Position)
                    {
                        reader.DiscardBufferedData();
                        stream.Seek(0, SeekOrigin.Begin);
                        lineBuffer = new LogLineBuffer();
                        continue;
                    }

                    if (PathReferencesDifferentFile(path, stream))
                    {
                        fileWasReplaced = true;
                        continue;
                    }

                    await Task.Delay(pollingInterval, cancellationToken);
                    continue;
                }

                IReadOnlyList<string> completedLines = lineBuffer.Append(characters.AsSpan(0, charactersRead));

                foreach (string line in completedLines)
                {
                    yield return line;
                }
            }
        }
    }

    private static bool PathReferencesDifferentFile(string path, FileStream openStream)
    {
        try
        {
            using FileStream pathStream = OpenFile(path);
            long openLength = openStream.Length;
            long pathLength = pathStream.Length;

            if (pathLength != openLength)
            {
                openLength = openStream.Length;
                pathLength = pathStream.Length;

                if (pathLength != openLength)
                {
                    return true;
                }
            }

            if (openLength == 0)
            {
                return false;
            }

            int sampleLength = (int)Math.Min(FileComparisonSampleSize, openLength);

            if (!SamplesMatch(openStream, pathStream, offset: 0, sampleLength))
            {
                return true;
            }

            long finalSampleOffset = openLength - sampleLength;

            return finalSampleOffset > 0
                && !SamplesMatch(openStream, pathStream, finalSampleOffset, sampleLength);
        }
        catch (FileNotFoundException)
        {
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            return false;
        }
    }

    private static bool SamplesMatch(
        FileStream openStream,
        FileStream pathStream,
        long offset,
        int sampleLength)
    {
        Span<byte> openSample = stackalloc byte[sampleLength];
        Span<byte> pathSample = stackalloc byte[sampleLength];
        int openBytesRead = RandomAccess.Read(openStream.SafeFileHandle, openSample, offset);
        int pathBytesRead = RandomAccess.Read(pathStream.SafeFileHandle, pathSample, offset);

        return openBytesRead == pathBytesRead
            && openSample[..openBytesRead].SequenceEqual(pathSample[..pathBytesRead]);
    }

    private static FileStream OpenFile(string path)
    {
        FileStreamOptions options = new()
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
            Share = FileShare.ReadWrite | FileShare.Delete,
            BufferSize = BufferSize,
            Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
        };

        return new FileStream(path, options);
    }

    private static StreamReader CreateReader(Stream stream) =>
        new(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            bufferSize: BufferSize,
            leaveOpen: true);
}
