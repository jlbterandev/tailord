using Tailord.Core;
using Xunit;

namespace Tailord.Core.Tests;

public sealed class LogTextClassifierTests
{
    [Theory]
    [InlineData("[DBG] Loading configuration", LogLevel.Debug)]
    [InlineData("2026-08-12 10:30:00 info Service started", LogLevel.Information)]
    [InlineData("WARN: Retrying request", LogLevel.Warning)]
    [InlineData("error | Connection failed", LogLevel.Error)]
    [InlineData("FATAL - Process terminated", LogLevel.Critical)]
    public void DetectLevel_RecognizesCommonMarkersIgnoringCase(string text, LogLevel expected)
    {
        LogLevel level = LogTextClassifier.DetectLevel(text);

        Assert.Equal(expected, level);
    }

    [Fact]
    public void DetectLevel_UsesFirstMarkerInTheLine()
    {
        LogLevel level = LogTextClassifier.DetectLevel("[WARN] Retrying after error response");

        Assert.Equal(LogLevel.Warning, level);
    }

    [Theory]
    [InlineData("Request completed normally")]
    [InlineData("Debugger attached")]
    [InlineData("")]
    public void DetectLevel_ReturnsUnknownWithoutACompleteMarker(string text)
    {
        LogLevel level = LogTextClassifier.DetectLevel(text);

        Assert.Equal(LogLevel.Unknown, level);
    }

    [Theory]
    [InlineData("System.InvalidOperationException: Invalid state", true)]
    [InlineData("EXCEPTION while reading the file", true)]
    [InlineData("The result was exceptional", false)]
    [InlineData("Request completed normally", false)]
    public void ContainsException_DetectsExceptionNames(string text, bool expected)
    {
        bool containsException = LogTextClassifier.ContainsException(text);

        Assert.Equal(expected, containsException);
    }
}
