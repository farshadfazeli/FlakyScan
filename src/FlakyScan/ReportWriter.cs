namespace FlakyScan;

public sealed class ReportWriter
{
    public void Write(IEnumerable<FlakyTest> tests, TextWriter output)
    {
        var flakyTests = tests.ToList();
        if (flakyTests.Count == 0)
        {
            output.WriteLine("No flaky tests found.");
            return;
        }

        output.WriteLine("| Test name | Passed | Failed | Failure rate |");
        output.WriteLine("| --- | ---: | ---: | ---: |");
        foreach (var test in flakyTests)
        {
            output.WriteLine($"| {test.TestName} | {test.Passed} | {test.Failed} | {test.FailureRate:P1} |");
        }
    }
}
