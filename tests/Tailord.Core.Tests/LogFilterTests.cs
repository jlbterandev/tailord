using Tailord.Core;
using Xunit;

namespace Tailord.Core.Tests;

public sealed class LogFilterTests
{
    [Fact]
    public void IsIncluded_ReturnsTrueWhenAnyIncludeRuleMatches()
    {
        TextFilterRule[] rules =
        [
            new("timeout"),
            new("10.20.30.40"),
        ];
        LogFilter filter = new(rules);
        LogEntry entry = new("Request to 10.20.30.40 completed", LogLevel.Information);

        bool isIncluded = filter.IsIncluded(entry);

        Assert.True(isIncluded);
    }

    [Fact]
    public void IsIncluded_ReturnsFalseWhenNoIncludeRuleMatches()
    {
        TextFilterRule[] rules =
        [
            new("timeout"),
            new("10.20.30.40"),
        ];
        LogFilter filter = new(rules);
        LogEntry entry = new("Request completed", LogLevel.Information);

        bool isIncluded = filter.IsIncluded(entry);

        Assert.False(isIncluded);
    }

    [Fact]
    public void IsIncluded_ReturnsTrueWhenThereAreNoIncludeRules()
    {
        TextFilterRule[] rules =
        [
            new("healthcheck", FilterRuleAction.Exclude),
        ];
        LogFilter filter = new(rules);
        LogEntry entry = new("Healthcheck completed", LogLevel.Information);

        bool isIncluded = filter.IsIncluded(entry);

        Assert.True(isIncluded);
    }
}
