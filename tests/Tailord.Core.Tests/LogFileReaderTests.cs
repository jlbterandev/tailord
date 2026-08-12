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
                .FollowAsync(
                    path,
                    TimeSpan.FromMilliseconds(10),
                    cancellationToken: cancellation.Token)
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
                .FollowAsync(
                    path,
                    TimeSpan.FromMilliseconds(10),
                    cancellationToken: cancellation.Token)
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
                .FollowAsync(
                    path,
                    TimeSpan.FromMilliseconds(10),
                    cancellationToken: cancellation.Token)
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
                .FollowAsync(
                    path,
                    TimeSpan.FromMilliseconds(10),
                    cancellationToken: cancellation.Token)
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

    [Fact]
    public async Task FollowAsync_ReopensFileAfterRotation()
    {
        string path = Path.GetTempFileName();
        string rotatedPath = $"{path}.rotated";

        try
        {
            await File.WriteAllTextAsync(path, "Original line\nOld partial line");
            LogFileReader reader = new();
            using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(5));
            await using IAsyncEnumerator<string> lines = reader
                .FollowAsync(
                    path,
                    TimeSpan.FromMilliseconds(10),
                    cancellationToken: cancellation.Token)
                .GetAsyncEnumerator();

            Assert.True(await lines.MoveNextAsync());
            Assert.Equal("Original line", lines.Current);

            Task<bool> lineAfterRotation = lines.MoveNextAsync().AsTask();
            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellation.Token);
            File.Move(path, rotatedPath);
            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellation.Token);
            Assert.False(lineAfterRotation.IsCompleted);

            await File.WriteAllTextAsync(path, "New file line\n", cancellation.Token);

            Assert.True(await lineAfterRotation);
            Assert.Equal("New file line", lines.Current);
        }
        finally
        {
            File.Delete(path);
            File.Delete(rotatedPath);
        }
    }

    [Fact]
    public async Task FollowAsync_FromEnd_IgnoresExistingContent()
    {
        string path = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(path, "Historical line\n");
            LogFileReader reader = new();
            using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(5));
            await using IAsyncEnumerator<string> lines = reader
                .FollowAsync(
                    path,
                    TimeSpan.FromMilliseconds(10),
                    LogFileStartPosition.End,
                    cancellation.Token)
                .GetAsyncEnumerator();

            Task<bool> newLine = lines.MoveNextAsync().AsTask();
            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellation.Token);
            Assert.False(newLine.IsCompleted);

            await File.AppendAllTextAsync(path, "Current line\n", cancellation.Token);

            Assert.True(await newLine);
            Assert.Equal("Current line", lines.Current);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task FollowAsync_ReadsBurstOfAppendedLinesInOrder()
    {
        string path = Path.GetTempFileName();

        try
        {
            const int lineCount = 100;
            string[] expectedLines = Enumerable.Range(1, lineCount)
                .Select(number => $"Line {number}")
                .ToArray();
            LogFileReader reader = new();
            using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(5));
            await using IAsyncEnumerator<string> lines = reader
                .FollowAsync(
                    path,
                    TimeSpan.FromMilliseconds(10),
                    LogFileStartPosition.End,
                    cancellation.Token)
                .GetAsyncEnumerator();
            Task<bool> firstLine = lines.MoveNextAsync().AsTask();

            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellation.Token);
            Assert.False(firstLine.IsCompleted);
            await File.AppendAllLinesAsync(path, expectedLines, cancellation.Token);

            List<string> actualLines = [];
            Assert.True(await firstLine);
            actualLines.Add(lines.Current);

            while (actualLines.Count < lineCount && await lines.MoveNextAsync())
            {
                actualLines.Add(lines.Current);
            }

            Assert.Equal(expectedLines, actualLines);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
