namespace Tailord.Core;

public static class LogTextClassifier
{
    private static readonly (string Token, LogLevel Level)[] LevelTokens =
    [
        ("debug", LogLevel.Debug),
        ("dbg", LogLevel.Debug),
        ("information", LogLevel.Information),
        ("info", LogLevel.Information),
        ("warning", LogLevel.Warning),
        ("warn", LogLevel.Warning),
        ("error", LogLevel.Error),
        ("err", LogLevel.Error),
        ("critical", LogLevel.Critical),
        ("crit", LogLevel.Critical),
        ("fatal", LogLevel.Critical),
    ];

    public static LogLevel DetectLevel(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        int firstMatchIndex = int.MaxValue;
        LogLevel detectedLevel = LogLevel.Unknown;

        foreach ((string token, LogLevel level) in LevelTokens)
        {
            int matchIndex = FindToken(text, token);

            if (matchIndex >= 0 && matchIndex < firstMatchIndex)
            {
                firstMatchIndex = matchIndex;
                detectedLevel = level;
            }
        }

        return detectedLevel;
    }

    public static bool ContainsException(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        const string exceptionToken = "exception";
        int searchIndex = 0;

        while (searchIndex < text.Length)
        {
            int matchIndex = text.IndexOf(exceptionToken, searchIndex, StringComparison.OrdinalIgnoreCase);

            if (matchIndex < 0)
            {
                return false;
            }

            int matchEnd = matchIndex + exceptionToken.Length;

            if (matchEnd == text.Length || !IsIdentifierCharacter(text[matchEnd]))
            {
                return true;
            }

            searchIndex = matchIndex + 1;
        }

        return false;
    }

    private static int FindToken(string text, string token)
    {
        int searchIndex = 0;

        while (searchIndex < text.Length)
        {
            int matchIndex = text.IndexOf(token, searchIndex, StringComparison.OrdinalIgnoreCase);

            if (matchIndex < 0)
            {
                return -1;
            }

            int matchEnd = matchIndex + token.Length;
            bool startsAtBoundary = matchIndex == 0 || !IsIdentifierCharacter(text[matchIndex - 1]);
            bool endsAtBoundary = matchEnd == text.Length || !IsIdentifierCharacter(text[matchEnd]);

            if (startsAtBoundary && endsAtBoundary)
            {
                return matchIndex;
            }

            searchIndex = matchIndex + 1;
        }

        return -1;
    }

    private static bool IsIdentifierCharacter(char value) => char.IsLetterOrDigit(value) || value == '_';
}
