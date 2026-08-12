namespace Tailord.Core;

public sealed class LogFilter
{
    private readonly TextFilterRule[] _rules;

    public LogFilter(IEnumerable<TextFilterRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        _rules = rules.ToArray();

        if (_rules.Any(rule => rule is null))
        {
            throw new ArgumentException("Filter rules cannot contain null values.", nameof(rules));
        }
    }

    public bool IsIncluded(LogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        bool hasInclusionRules = false;

        foreach (TextFilterRule rule in _rules)
        {
            if (rule.Action != FilterRuleAction.Include)
            {
                continue;
            }

            hasInclusionRules = true;

            if (rule.Matches(entry))
            {
                return true;
            }
        }

        return !hasInclusionRules;
    }
}
