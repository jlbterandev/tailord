using Tailord.Core;

return Run(args);

static int Run(string[] args)
{
    if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
    {
        PrintHelp();
        return 0;
    }

    if (args.Contains("--version"))
    {
        Console.WriteLine(typeof(TailordProduct).Assembly.GetName().Version?.ToString(3) ?? "0.0.0");
        return 0;
    }

    Console.Error.WriteLine("Log reading will be added in the next milestone.");
    Console.Error.WriteLine("Run 'tailord --help' to see the current commands.");
    return 2;
}

static void PrintHelp()
{
    Console.WriteLine($$"""
        {{TailordProduct.Name}}
        {{TailordProduct.Description}}

        Usage:
          tailord --help
          tailord --version

        The file-following command will be introduced in the next milestone.
        """);
}

