namespace Tailord.Core;

public sealed class LogFilter
{
    private readonly TextFilterRule[] _rules;

    public LogFilter(
        IEnumerable<TextFilterRule> rules,
        InclusionMatchMode inclusionMatchMode = InclusionMatchMode.Any)
    {
        ArgumentNullException.ThrowIfNull(rules);

        if (!Enum.IsDefined(inclusionMatchMode))
        {
            throw new ArgumentOutOfRangeException(nameof(inclusionMatchMode));
        }

        _rules = rules.ToArray();

        if (_rules.Any(rule => rule is null))
        {
            throw new ArgumentException("Filter rules cannot contain null values.", nameof(rules));
        }

        InclusionMode = inclusionMatchMode;
    }

    public InclusionMatchMode InclusionMode { get; }

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
            bool matches = rule.Matches(entry);

            if (InclusionMode == InclusionMatchMode.Any && matches)
            {
                return true;
            }

            if (InclusionMode == InclusionMatchMode.All && !matches)
            {
                return false;
            }
        }

        return InclusionMode == InclusionMatchMode.All || !hasInclusionRules;
    }

    public bool IsVisible(LogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (!IsIncluded(entry))
        {
            return false;
        }

        foreach (TextFilterRule rule in _rules)
        {
            if (rule.Action == FilterRuleAction.Exclude && rule.Matches(entry))
            {
                return false;
            }
        }

        return true;
    }
}
