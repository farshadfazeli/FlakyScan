using FlakyScan;

namespace FlakyScan.Tests;

public sealed class TrxParserTests
{
    [Fact]
    public void Parse_reads_test_names_and_outcomes()
    {
        var results = new TrxParser().Parse(SampleTrx);

        Assert.Equal(3, results.Count);
        Assert.Equal(new TestResult("App.Tests.Works", TestOutcome.Passed), results[0]);
        Assert.Equal(new TestResult("App.Tests.Breaks", TestOutcome.Failed), results[1]);
        Assert.Equal(new TestResult("App.Tests.Skipped", TestOutcome.Ignored), results[2]);
    }

    [Fact]
    public void Parse_uses_test_name_when_unit_test_definition_is_missing()
    {
        const string trx = """
            <TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
              <Results><UnitTestResult testId="missing" testName="Fallback.Name" outcome="Passed" /></Results>
            </TestRun>
            """;

        var result = Assert.Single(new TrxParser().Parse(trx));

        Assert.Equal("Fallback.Name", result.TestName);
    }

    private const string SampleTrx = """
        <TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
          <TestDefinitions>
            <UnitTest name="App.Tests.Works" id="1" />
            <UnitTest name="App.Tests.Breaks" id="2" />
            <UnitTest name="App.Tests.Skipped" id="3" />
          </TestDefinitions>
          <Results>
            <UnitTestResult testId="1" outcome="Passed" />
            <UnitTestResult testId="2" outcome="Failed" />
            <UnitTestResult testId="3" outcome="NotExecuted" />
          </Results>
        </TestRun>
        """;
}
