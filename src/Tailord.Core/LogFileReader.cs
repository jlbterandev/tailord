using System.Runtime.CompilerServices;
using System.Text;

namespace Tailord.Core;

public sealed class LogFileReader
{
    private const int BufferSize = 4096;

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

        await using FileStream stream = OpenFile(path);
        using StreamReader reader = CreateReader(stream);
        char[] characters = new char[BufferSize];
        LogLineBuffer lineBuffer = new();

        while (true)
        {
            int charactersRead = await reader.ReadAsync(characters, cancellationToken);

            if (charactersRead == 0)
            {
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
