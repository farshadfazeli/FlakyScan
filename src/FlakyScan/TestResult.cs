namespace FlakyScan;

public enum TestOutcome
{
    Passed,
    Failed,
    Ignored
}

public sealed record TestResult(string TestName, TestOutcome Outcome);
