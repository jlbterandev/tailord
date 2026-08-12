using System.Text.RegularExpressions;

namespace Tailord.Core;

public sealed class TextFilterRule
{
    private static readonly TimeSpan RegularExpressionTimeout = TimeSpan.FromMilliseconds(100);

    private readonly Regex? _regularExpression;

    public TextFilterRule(
        string text,
        FilterRuleAction action = FilterRuleAction.Include,
        bool caseSensitive = false,
        TextFilterMatchMode matchMode = TextFilterMatchMode.PlainText)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);

        if (!Enum.IsDefined(matchMode))
        {
            throw new ArgumentOutOfRangeException(nameof(matchMode));
        }

        Text = text;
        Action = action;
        CaseSensitive = caseSensitive;
        MatchMode = matchMode;

        if (matchMode == TextFilterMatchMode.RegularExpression)
        {
            RegexOptions options = RegexOptions.CultureInvariant;

            if (!caseSensitive)
            {
                options |= RegexOptions.IgnoreCase;
            }

            try
            {
                _regularExpression = new Regex(text, options, RegularExpressionTimeout);
            }
            catch (ArgumentException exception)
            {
                throw new ArgumentException("The regular expression is invalid.", nameof(text), exception);
            }
        }
    }

    public string Text { get; }

    public FilterRuleAction Action { get; }

    public bool CaseSensitive { get; }

    public TextFilterMatchMode MatchMode { get; }

    public bool Matches(LogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (_regularExpression is not null)
        {
            try
            {
                return _regularExpression.IsMatch(entry.Message);
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        StringComparison comparison = CaseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        return entry.Message.Contains(Text, comparison);
    }
}
