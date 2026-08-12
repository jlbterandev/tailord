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

    if (args.Length != 1 || args[0].StartsWith('-'))
    {
        Console.Error.WriteLine("tailord: expected a single log file path.");
        Console.Error.WriteLine("Run 'tailord --help' for usage information.");
        return 2;
    }

    string path = args[0];
    LogFileReader reader = new();

    try
    {
        await foreach (string line in reader.ReadExistingAsync(path))
        {
            Console.WriteLine(line);
        }

        return 0;
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
    {
        Console.Error.WriteLine($"tailord: cannot read '{path}': {exception.Message}");
        return 1;
    }
}

static void PrintHelp()
{
    Console.WriteLine($$"""
        {{TailordProduct.Name}}
        {{TailordProduct.Description}}

        Usage:
          tailord <file>
          tailord --help
          tailord --version

        Prints the existing lines from a log file. Following changes will be
        introduced in a later increment.
        """);
}
