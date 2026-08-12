namespace Tailord.Core;

public sealed class TextFilterRule
{
    public TextFilterRule(string text, bool caseSensitive = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);

        Text = text;
        CaseSensitive = caseSensitive;
    }

    public string Text { get; }

    public bool CaseSensitive { get; }

    public bool Matches(LogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        StringComparison comparison = CaseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        return entry.Message.Contains(Text, comparison);
    }
}
