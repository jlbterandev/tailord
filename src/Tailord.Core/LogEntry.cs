namespace Tailord.Core;

public sealed record LogEntry(
    string Message,
    LogLevel Level,
    DateTimeOffset? Timestamp = null,
    bool IsException = false);
