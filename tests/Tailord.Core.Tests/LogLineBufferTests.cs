using Tailord.Core;
using Xunit;

namespace Tailord.Core.Tests;

public sealed class LogLineBufferTests
{
    [Fact]
    public void Append_EmitsCompleteLinesAndKeepsPartialLine()
    {
        LogLineBuffer buffer = new();

        IReadOnlyList<string> completedLines = buffer.Append("First line\nSecond");

        Assert.Equal(["First line"], completedLines);
        Assert.Equal("Second", buffer.PartialLine);
    }

    [Fact]
    public void Append_CompletesPartialLineWithLaterText()
    {
        LogLineBuffer buffer = new();
        buffer.Append("First part");

        IReadOnlyList<string> completedLines = buffer.Append(" and second part\nNext line");

        Assert.Equal(["First part and second part"], completedLines);
        Assert.Equal("Next line", buffer.PartialLine);
    }

    [Fact]
    public void Append_HandlesCarriageReturnAndLineFeedAcrossChunks()
    {
        LogLineBuffer buffer = new();

        IReadOnlyList<string> firstChunkLines = buffer.Append("First line\r");
        IReadOnlyList<string> secondChunkLines = buffer.Append("\nSecond line\r\n");

        Assert.Empty(firstChunkLines);
        Assert.Equal(["First line", "Second line"], secondChunkLines);
        Assert.Equal(string.Empty, buffer.PartialLine);
    }
}
