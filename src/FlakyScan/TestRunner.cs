using System.ComponentModel;
using System.Diagnostics;

namespace FlakyScan;

public sealed class TestRunner
{
    private readonly TrxParser parser;

    public TestRunner(TrxParser parser) => this.parser = parser;

    public async Task<IReadOnlyList<IReadOnlyList<TestResult>>> RunAsync(string path, int runCount, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath) && !Directory.Exists(fullPath))
        {
            throw new FileNotFoundException($"Test path does not exist: {path}");
        }

        var targetPath = Directory.Exists(fullPath) ? FindTestTarget(fullPath) : fullPath;
        var workingDirectory = Directory.Exists(fullPath) ? fullPath : Path.GetDirectoryName(fullPath)!;
        var resultsDirectory = Path.Combine(Path.GetTempPath(), $"flakyscan-{Guid.NewGuid():N}");
        Directory.CreateDirectory(resultsDirectory);

        try
        {
            var allResults = new List<IReadOnlyList<TestResult>>();
            for (var run = 1; run <= runCount; run++)
            {
                var trxPath = Path.Combine(resultsDirectory, $"run_{run}.trx");
                await RunDotnetTestAsync(targetPath, workingDirectory, resultsDirectory, run, trxPath, cancellationToken);
                if (!File.Exists(trxPath))
                {
                    throw new InvalidOperationException($"Run {run} completed without producing a TRX file. The test project may have failed to build.");
                }

                allResults.Add(parser.Parse(await File.ReadAllTextAsync(trxPath, cancellationToken)));
            }

            return allResults;
        }
        finally
        {
            Directory.Delete(resultsDirectory, recursive: true);
        }
    }

    private static string FindTestTarget(string directory)
    {
        var targets = Directory.EnumerateFiles(directory, "*.*", SearchOption.TopDirectoryOnly)
            .Where(file => Path.GetExtension(file) is ".sln" or ".slnx" or ".csproj")
            .ToList();
        var solutions = targets.Where(file => Path.GetExtension(file) is ".sln" or ".slnx").ToList();
        if (solutions.Count == 1)
        {
            return solutions[0];
        }

        return targets.Count switch
        {
            1 => targets[0],
            0 => throw new InvalidOperationException($"No .sln, .slnx, or .csproj file was found in: {directory}"),
            _ => throw new InvalidOperationException($"More than one test target was found in: {directory}. Pass the project or solution path directly.")
        };
    }

    private static async Task RunDotnetTestAsync(string path, string workingDirectory, string resultsDirectory, int run, string trxPath, CancellationToken cancellationToken)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.StartInfo.ArgumentList.Add("test");
        process.StartInfo.ArgumentList.Add(path);
        process.StartInfo.ArgumentList.Add("--logger");
        process.StartInfo.ArgumentList.Add($"trx;LogFileName=run_{run}.trx");
        process.StartInfo.ArgumentList.Add("--results-directory");
        process.StartInfo.ArgumentList.Add(resultsDirectory);
        process.StartInfo.ArgumentList.Add("--no-restore");
        process.StartInfo.ArgumentList.Add("--nologo");

        try
        {
            process.Start();
        }
        catch (Win32Exception exception)
        {
            throw new InvalidOperationException("dotnet was not found. Install the .NET SDK and try again.", exception);
        }

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        var output = await outputTask;
        var error = await errorTask;
        if (process.ExitCode != 0 && !File.Exists(trxPath))
        {
            var details = string.IsNullOrWhiteSpace(error) ? output : error;
            throw new InvalidOperationException($"dotnet test failed on run {run}, likely during build: {details.Trim()}");
        }
    }
}
