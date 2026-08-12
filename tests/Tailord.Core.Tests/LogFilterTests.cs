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

    [Fact]
    public void IsIncluded_WithAllMode_ReturnsTrueWhenEveryIncludeRuleMatches()
    {
        TextFilterRule[] rules =
        [
            new("timeout"),
            new("10.20.30.40"),
        ];
        LogFilter filter = new(rules, InclusionMatchMode.All);
        LogEntry entry = new("Timeout while contacting 10.20.30.40", LogLevel.Warning);

        bool isIncluded = filter.IsIncluded(entry);

        Assert.True(isIncluded);
        Assert.Equal(InclusionMatchMode.All, filter.InclusionMode);
    }

    [Fact]
    public void IsIncluded_WithAllMode_ReturnsFalseWhenAnyIncludeRuleDoesNotMatch()
    {
        TextFilterRule[] rules =
        [
            new("timeout"),
            new("10.20.30.40"),
        ];
        LogFilter filter = new(rules, InclusionMatchMode.All);
        LogEntry entry = new("Timeout while contacting the server", LogLevel.Warning);

        bool isIncluded = filter.IsIncluded(entry);

        Assert.False(isIncluded);
    }
}
