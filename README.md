# FlakyScan

FlakyScan runs a .NET test project or solution several times and reports tests that pass in some runs and fail in others.

## Run it

Requires the .NET 8 SDK.

```bash
dotnet build FlakyScan.sln
dotnet run --project src/FlakyScan -- --path ./samples/FlakySample/FlakySample.csproj --runs 5
```

Use `--fail-on-flaky` when a flaky test should make the command exit with code `1`. The default is five runs; use `--runs N` to choose another positive number.

Example output from the sample project:

```text
| Test name | Passed | Failed | Failure rate |
| --- | ---: | ---: | ---: |
| FlakySample.FlakySampleTests.Randomly_flaky_test | 3 | 2 | 40.0% |
| FlakySample.FlakySampleTests.Timing_flaky_test | 4 | 1 | 20.0% |
```

The sample is intentionally nondeterministic, so its exact counts will vary.

## How it works

For every run, FlakyScan starts `dotnet test` with a TRX logger and stores the result in a temporary folder. It reads the TRX files with `System.Xml.Linq`, matches results by fully qualified test name, ignores skipped tests, and prints only tests with both a pass and a failure.

## Limitations

FlakyScan does not create HTML or JSON reports, analyze failure patterns, run tests in parallel, or read configuration files. It is a source project rather than a packaged `dotnet tool`.

