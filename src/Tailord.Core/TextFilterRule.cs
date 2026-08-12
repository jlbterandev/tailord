namespace Tailord.Core;

public sealed class TextFilterRule
{
    public TextFilterRule(string text)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);

        Text = text;
    }

    public string Text { get; }

    public bool Matches(LogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return entry.Message.Contains(Text, StringComparison.OrdinalIgnoreCase);
    }
}
