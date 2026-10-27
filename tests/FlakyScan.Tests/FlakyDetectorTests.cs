using FlakyScan;

namespace FlakyScan.Tests;

public sealed class FlakyDetectorTests
{
    private readonly FlakyDetector detector = new();

    [Fact]
    public void All_pass_tests_are_not_flaky()
    {
        var result = detector.FindFlakyTests(Runs(TestOutcome.Passed, TestOutcome.Passed));

        Assert.Empty(result);
    }

    [Fact]
    public void All_fail_tests_are_not_flaky()
    {
        var result = detector.FindFlakyTests(Runs(TestOutcome.Failed, TestOutcome.Failed));

        Assert.Empty(result);
    }

    [Fact]
    public void Mixed_results_are_reported_with_counts()
    {
        var result = Assert.Single(detector.FindFlakyTests(Runs(TestOutcome.Passed, TestOutcome.Failed, TestOutcome.Failed)));

        Assert.Equal(new FlakyTest("Example.Test", 1, 2), result);
        Assert.Equal(2d / 3d, result.FailureRate);
    }

    [Fact]
    public void Skipped_tests_are_ignored()
    {
        var result = detector.FindFlakyTests(Runs(TestOutcome.Passed, TestOutcome.Ignored));

        Assert.Empty(result);
    }

    private static IEnumerable<IEnumerable<TestResult>> Runs(params TestOutcome[] outcomes)
    {
        return outcomes.Select(outcome => new[] { new TestResult("Example.Test", outcome) });
    }
}
