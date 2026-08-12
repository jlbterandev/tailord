using Tailord.Core;
using Xunit;

namespace Tailord.Core.Tests;

public sealed class LogEntryTests
{
    [Fact]
    public void Constructor_PreservesEntryData()
    {
        DateTimeOffset timestamp = new(2026, 8, 12, 10, 30, 0, TimeSpan.FromHours(2));

        LogEntry entry = new("Unhandled connection failure", LogLevel.Error, timestamp, true);

        Assert.Equal("Unhandled connection failure", entry.Message);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Equal(timestamp, entry.Timestamp);
        Assert.True(entry.IsException);
    }

    [Fact]
    public void OptionalData_DefaultsToNoTimestampAndNoException()
    {
        LogEntry entry = new("Service started", LogLevel.Information);

        Assert.Null(entry.Timestamp);
        Assert.False(entry.IsException);
    }

    [Fact]
    public void Entries_WithTheSameData_AreEqual()
    {
        DateTimeOffset timestamp = new(2026, 8, 12, 10, 30, 0, TimeSpan.FromHours(2));
        LogEntry first = new("Unhandled connection failure", LogLevel.Error, timestamp, true);
        LogEntry second = new("Unhandled connection failure", LogLevel.Error, timestamp, true);

        Assert.Equal(first, second);
    }
}
