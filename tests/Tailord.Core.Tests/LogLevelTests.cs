using Tailord.Core;
using Xunit;

namespace Tailord.Core.Tests;

public sealed class LogLevelTests
{
    [Fact]
    public void Values_AreOrderedByIncreasingSeverity()
    {
        LogLevel[] expectedLevels =
        [
            LogLevel.Unknown,
            LogLevel.Debug,
            LogLevel.Information,
            LogLevel.Warning,
            LogLevel.Error,
            LogLevel.Critical,
        ];

        Assert.Equal(expectedLevels, Enum.GetValues<LogLevel>());
        Assert.Equal(Enumerable.Range(0, expectedLevels.Length), expectedLevels.Select(level => (int)level));
    }
}
