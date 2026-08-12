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

    [Fact]
    public async Task FollowAsync_ReadsExistingAndAppendedCompleteLines()
    {
        string path = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(path, "Existing line\n");
            LogFileReader reader = new();
            using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(5));
            await using IAsyncEnumerator<string> lines = reader
                .FollowAsync(path, TimeSpan.FromMilliseconds(10), cancellation.Token)
                .GetAsyncEnumerator();

            Assert.True(await lines.MoveNextAsync());
            Assert.Equal("Existing line", lines.Current);

            Task<bool> appendedLine = lines.MoveNextAsync().AsTask();
            await File.AppendAllTextAsync(path, "Appended line\n", cancellation.Token);

            Assert.True(await appendedLine);
            Assert.Equal("Appended line", lines.Current);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task FollowAsync_WaitsForAPartialLineToBeCompleted()
    {
        string path = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(path, "Partial");
            LogFileReader reader = new();
            using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(5));
            await using IAsyncEnumerator<string> lines = reader
                .FollowAsync(path, TimeSpan.FromMilliseconds(10), cancellation.Token)
                .GetAsyncEnumerator();

            Task<bool> completedLine = lines.MoveNextAsync().AsTask();
            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellation.Token);
            Assert.False(completedLine.IsCompleted);

            await File.AppendAllTextAsync(path, " line\n", cancellation.Token);

            Assert.True(await completedLine);
            Assert.Equal("Partial line", lines.Current);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task FollowAsync_CanBeCancelledWhileWaitingForContent()
    {
        string path = Path.GetTempFileName();

        try
        {
            LogFileReader reader = new();
            using CancellationTokenSource cancellation = new();
            await using IAsyncEnumerator<string> lines = reader
                .FollowAsync(path, TimeSpan.FromMilliseconds(10), cancellation.Token)
                .GetAsyncEnumerator();
            Task<bool> pendingLine = lines.MoveNextAsync().AsTask();

            cancellation.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await pendingLine);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task FollowAsync_RestartsAndDiscardsPartialLineAfterTruncation()
    {
        string path = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(
                path,
                "Original completed line with enough text\nOld partial line with enough text");
            LogFileReader reader = new();
            using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(5));
            await using IAsyncEnumerator<string> lines = reader
                .FollowAsync(path, TimeSpan.FromMilliseconds(10), cancellation.Token)
                .GetAsyncEnumerator();

            Assert.True(await lines.MoveNextAsync());
            Assert.Equal("Original completed line with enough text", lines.Current);

            Task<bool> lineAfterTruncation = lines.MoveNextAsync().AsTask();
            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellation.Token);
            await File.WriteAllTextAsync(path, "New line\n", cancellation.Token);

            Assert.True(await lineAfterTruncation);
            Assert.Equal("New line", lines.Current);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
