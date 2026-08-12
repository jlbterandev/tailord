using Tailord.Core;
using Xunit;

namespace Tailord.Core.Tests;

public sealed class TextFilterRuleTests
{
    [Theory]
    [InlineData("Request timed out after 30 seconds", "timed out")]
    [InlineData("Connection TIMEOUT", "timeout")]
    [InlineData("Server 10.20.30.40 is unavailable", "10.20.30.40")]
    public void Matches_ReturnsTrueWhenMessageContainsText(string message, string filterText)
    {
        TextFilterRule rule = new(filterText);
        LogEntry entry = new(message, LogLevel.Information);

        bool matches = rule.Matches(entry);

        Assert.True(matches);
    }

    [Fact]
    public void Matches_ReturnsFalseWhenMessageDoesNotContainText()
    {
        TextFilterRule rule = new("error");
        LogEntry entry = new("Request completed", LogLevel.Error);

        bool matches = rule.Matches(entry);

        Assert.False(matches);
    }

    [Fact]
    public void Constructor_DefaultsToAnIncludeRule()
    {
        TextFilterRule rule = new("timeout");

        Assert.Equal(FilterRuleAction.Include, rule.Action);
    }

    [Fact]
    public void Constructor_PreservesExcludeAction()
    {
        TextFilterRule rule = new("healthcheck", FilterRuleAction.Exclude);

        Assert.Equal(FilterRuleAction.Exclude, rule.Action);
    }

    [Theory]
    [InlineData("Connection TIMEOUT", "TIMEOUT", true)]
    [InlineData("Connection TIMEOUT", "timeout", false)]
    public void Matches_RespectsCaseWhenRequested(string message, string filterText, bool expected)
    {
        TextFilterRule rule = new(filterText, caseSensitive: true);
        LogEntry entry = new(message, LogLevel.Warning);

        bool matches = rule.Matches(entry);

        Assert.Equal(expected, matches);
        Assert.True(rule.CaseSensitive);
    }

    [Fact]
    public void Constructor_RejectsEmptyText()
    {
        Assert.Throws<ArgumentException>(() => new TextFilterRule(string.Empty));
    }
}
