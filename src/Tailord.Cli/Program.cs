using Tailord.Core;

return await RunAsync(args);

static async Task<int> RunAsync(string[] args)
{
    if (args.Length == 0)
    {
        PrintHelp();
        return 0;
    }

    if (args is ["--help"] or ["-h"])
    {
        PrintHelp();
        return 0;
    }

    if (args is ["--version"])
    {
        Console.WriteLine(typeof(TailordProduct).Assembly.GetName().Version?.ToString(3) ?? "0.0.0");
        return 0;
    }

    string? path = null;
    bool follow = false;
    bool fromEnd = false;
    HashSet<LogLevel>? visibleLevels = null;
    string? includeText = null;

    for (int index = 0; index < args.Length; index++)
    {
        string argument = args[index];

        switch (argument)
        {
            case "--follow":
                if (follow)
                {
                    return PrintUsageError("--follow can only be specified once.");
                }

                follow = true;
                break;

            case "--from-end":
                if (fromEnd)
                {
                    return PrintUsageError("--from-end can only be specified once.");
                }

                fromEnd = true;
                break;

            case "--level":
                if (visibleLevels is not null)
                {
                    return PrintUsageError("--level can only be specified once.");
                }

                if (++index >= args.Length)
                {
                    return PrintUsageError("--level requires a comma-separated list.");
                }

                if (!TryParseLevels(args[index], out visibleLevels, out string? invalidLevel))
                {
                    return PrintUsageError($"unknown log level '{invalidLevel}'.");
                }

                break;

            case "--include":
                if (includeText is not null)
                {
                    return PrintUsageError("--include can only be specified once.");
                }

                if (++index >= args.Length || args[index].Length == 0)
                {
                    return PrintUsageError("--include requires text to match.");
                }

                includeText = args[index];
                break;

            default:
                if (argument.StartsWith('-') || path is not null)
                {
                    return PrintUsageError("expected a single log file path.");
                }

                path = argument;
                break;
        }
    }

    if (path is null)
    {
        return PrintUsageError("expected a single log file path.");
    }

    if (fromEnd && !follow)
    {
        return PrintUsageError("--from-end requires --follow.");
    }

    LogFileReader reader = new();
    TextFilterRule[] rules = includeText is null
        ? []
        : [new TextFilterRule(includeText)];
    LogFilter? filter = visibleLevels is null && rules.Length == 0
        ? null
        : new LogFilter(rules, visibleLevels: visibleLevels);
    using CancellationTokenSource cancellation = new();

    ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
    {
        eventArgs.Cancel = true;
        cancellation.Cancel();
    };

    try
    {
        IAsyncEnumerable<string> lines = follow
            ? reader.FollowAsync(
                path,
                TimeSpan.FromMilliseconds(100),
                fromEnd ? LogFileStartPosition.End : LogFileStartPosition.Beginning,
                cancellationToken: cancellation.Token)
            : reader.ReadExistingAsync(path);

        if (follow)
        {
            Console.CancelKeyPress += cancelHandler;
        }

        await foreach (string line in lines)
        {
            LogEntry entry = new(line, LogTextClassifier.DetectLevel(line));

            if (filter is null || filter.IsVisible(entry))
            {
                Console.WriteLine(line);
            }
        }

        return 0;
    }
    catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
    {
        return 0;
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
    {
        Console.Error.WriteLine($"tailord: cannot read '{path}': {exception.Message}");
        return 1;
    }
    finally
    {
        if (follow)
        {
            Console.CancelKeyPress -= cancelHandler;
        }
    }
}

static bool TryParseLevels(
    string value,
    out HashSet<LogLevel> levels,
    out string? invalidLevel)
{
    levels = [];
    invalidLevel = null;

    foreach (string name in value.Split(',', StringSplitOptions.TrimEntries))
    {
        bool isKnownLevel = name.Length > 0
            && Enum.GetNames<LogLevel>()
                .Any(knownName => string.Equals(knownName, name, StringComparison.OrdinalIgnoreCase));

        if (!isKnownLevel)
        {
            invalidLevel = name;
            return false;
        }

        levels.Add(Enum.Parse<LogLevel>(name, ignoreCase: true));
    }

    return true;
}

static int PrintUsageError(string message)
{
    Console.Error.WriteLine($"tailord: {message}");
    Console.Error.WriteLine("Run 'tailord --help' for usage information.");
    return 2;
}

static void PrintHelp()
{
    Console.WriteLine($$"""
        {{TailordProduct.Name}}
        {{TailordProduct.Description}}

        Usage:
          tailord <file>
          tailord <file> --follow
          tailord <file> --follow --from-end
          tailord <file> --level <level[,level...]>
          tailord <file> --include <text>
          tailord --help
          tailord --version

        Prints lines from a log file. Use --follow to wait for new lines and
        Ctrl+C to stop. Add --from-end to ignore lines that already exist when
        following starts. Available levels: unknown, debug, information,
        warning, error, critical. --include performs a case-insensitive text
        match.
        """);
}
