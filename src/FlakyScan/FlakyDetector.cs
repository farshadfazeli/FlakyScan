namespace FlakyScan;

public sealed record FlakyTest(string TestName, int Passed, int Failed)
{
    public double FailureRate => (double)Failed / (Passed + Failed);
}

public sealed class FlakyDetector
{
    public IReadOnlyList<FlakyTest> FindFlakyTests(IEnumerable<IEnumerable<TestResult>> runs)
    {
        ArgumentNullException.ThrowIfNull(runs);

        return runs
            .SelectMany(run => run)
            .Where(result => result.Outcome is TestOutcome.Passed or TestOutcome.Failed)
            .GroupBy(result => result.TestName)
            .Select(group => new FlakyTest(
                group.Key,
                group.Count(result => result.Outcome == TestOutcome.Passed),
                group.Count(result => result.Outcome == TestOutcome.Failed)))
            .Where(test => test.Passed > 0 && test.Failed > 0)
            .OrderByDescending(test => test.FailureRate)
            .ThenBy(test => test.TestName)
            .ToList();
    }
}
