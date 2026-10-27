using System.Xml.Linq;

namespace FlakyScan;

public sealed class TrxParser
{
    private static readonly XNamespace TestNamespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";

    public IReadOnlyList<TestResult> Parse(string trxContent)
    {
        ArgumentException.ThrowIfNullOrEmpty(trxContent);

        var document = XDocument.Parse(trxContent);
        var testNames = document
            .Descendants(TestNamespace + "UnitTest")
            .ToDictionary(
                element => (string?)element.Attribute("id") ?? string.Empty,
                element => (string?)element.Attribute("name") ?? string.Empty);

        return document
            .Descendants(TestNamespace + "UnitTestResult")
            .Select(result => CreateResult(result, testNames))
            .Where(result => result is not null)
            .Select(result => result!)
            .ToList();
    }

    private static TestResult? CreateResult(XElement result, IReadOnlyDictionary<string, string> testNames)
    {
        var testId = (string?)result.Attribute("testId") ?? string.Empty;
        var testName = testNames.GetValueOrDefault(testId)
            ?? (string?)result.Attribute("testName");
        var outcome = (string?)result.Attribute("outcome");

        if (string.IsNullOrWhiteSpace(testName) || outcome is null)
        {
            return null;
        }

        var mappedOutcome = outcome switch
        {
            "Passed" => TestOutcome.Passed,
            "Failed" => TestOutcome.Failed,
            _ => TestOutcome.Ignored
        };

        return new TestResult(testName, mappedOutcome);
    }
}
