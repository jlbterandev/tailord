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

    int followOptionCount = args.Count(argument => argument == "--follow");
    int fromEndOptionCount = args.Count(argument => argument == "--from-end");
    string[] paths = args
        .Where(argument => argument is not "--follow" and not "--from-end")
        .ToArray();

    if (followOptionCount > 1
        || fromEndOptionCount > 1
        || paths.Length != 1
        || paths[0].StartsWith('-'))
    {
        Console.Error.WriteLine("tailord: expected a single log file path.");
        Console.Error.WriteLine("Run 'tailord --help' for usage information.");
        return 2;
    }

    string path = paths[0];
    bool follow = followOptionCount == 1;
    bool fromEnd = fromEndOptionCount == 1;

    if (fromEnd && !follow)
    {
        Console.Error.WriteLine("tailord: --from-end requires --follow.");
        Console.Error.WriteLine("Run 'tailord --help' for usage information.");
        return 2;
    }

    LogFileReader reader = new();
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
            Console.WriteLine(line);
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

static void PrintHelp()
{
    Console.WriteLine($$"""
        {{TailordProduct.Name}}
        {{TailordProduct.Description}}

        Usage:
          tailord <file>
          tailord <file> --follow
          tailord <file> --follow --from-end
          tailord --help
          tailord --version

        Prints lines from a log file. Use --follow to wait for new lines and
        Ctrl+C to stop. Add --from-end to ignore lines that already exist when
        following starts.
        """);
}
