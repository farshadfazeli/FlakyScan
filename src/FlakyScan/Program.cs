namespace FlakyScan;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var options = ParseArguments(args);
            var runs = await new TestRunner(new TrxParser()).RunAsync(options.Path, options.Runs);
            var flakyTests = new FlakyDetector().FindFlakyTests(runs);
            new ReportWriter().Write(flakyTests, Console.Out);
            return options.FailOnFlaky && flakyTests.Count > 0 ? 1 : 0;
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or FileNotFoundException)
        {
            Console.Error.WriteLine($"Error: {exception.Message}");
            return 2;
        }
    }

    private static Options ParseArguments(string[] args)
    {
        string? path = null;
        var runs = 5;
        var failOnFlaky = false;

        for (var index = 0; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "--path" when index + 1 < args.Length:
                    path = args[++index];
                    break;
                case "--runs" when index + 1 < args.Length && int.TryParse(args[++index], out runs) && runs > 0:
                    break;
                case "--fail-on-flaky":
                    failOnFlaky = true;
                    break;
                default:
                    throw new ArgumentException($"Unknown or incomplete argument: {args[index]}");
            }
        }

        return path is null
            ? throw new ArgumentException("The --path option is required.")
            : new Options(path, runs, failOnFlaky);
    }

    private sealed record Options(string Path, int Runs, bool FailOnFlaky);
}
