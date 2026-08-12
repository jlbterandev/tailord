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

        FileStreamOptions options = new()
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
            Share = FileShare.ReadWrite | FileShare.Delete,
            BufferSize = BufferSize,
            Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
        };

        await using FileStream stream = new(path, options);
        using StreamReader reader = new(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            bufferSize: BufferSize);

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            yield return line;
        }
    }
}
