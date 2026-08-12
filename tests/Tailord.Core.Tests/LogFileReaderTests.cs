using Tailord.Core;
using Xunit;

namespace Tailord.Core.Tests;

public sealed class LogFileReaderTests
{
    [Fact]
    public async Task ReadExistingAsync_ReturnsLinesInOrder()
    {
        string path = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(path, "First line\nSecond line\nThird line");
            LogFileReader reader = new();
            List<string> lines = [];

            await foreach (string line in reader.ReadExistingAsync(path))
            {
                lines.Add(line);
            }

            string[] expectedLines = ["First line", "Second line", "Third line"];
            Assert.Equal(expectedLines, lines);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ReadExistingAsync_ReturnsNoLinesForAnEmptyFile()
    {
        string path = Path.GetTempFileName();

        try
        {
            LogFileReader reader = new();
            List<string> lines = [];

            await foreach (string line in reader.ReadExistingAsync(path))
            {
                lines.Add(line);
            }

            Assert.Empty(lines);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ReadExistingAsync_HonorsCancellation()
    {
        string path = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(path, "First line\nSecond line");
            LogFileReader reader = new();
            using CancellationTokenSource cancellation = new();
            cancellation.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                await foreach (string _ in reader.ReadExistingAsync(path, cancellation.Token))
                {
                }
            });
        }
        finally
        {
            File.Delete(path);
        }
    }
}
