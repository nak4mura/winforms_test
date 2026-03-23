using WinFormsE2E.Models;

namespace WinFormsE2E.Reporting;

public class ConsoleReporter : IResultReporter
{
    private readonly bool _verbose;

    public ConsoleReporter(bool verbose = false)
    {
        _verbose = verbose;
    }

    public void ReportScenarioStart(string scenarioName)
    {
        Console.WriteLine();
        Console.WriteLine($"  Scenario: {scenarioName}");
    }

    public void ReportStepResult(StepResult result)
    {
        var (color, label) = result.Outcome switch
        {
            StepOutcome.Passed => (ConsoleColor.Green, "PASS"),
            StepOutcome.Failed => (ConsoleColor.Red, "FAIL"),
            StepOutcome.Error => (ConsoleColor.Yellow, "ERROR"),
            StepOutcome.Skipped => (ConsoleColor.DarkGray, "SKIP"),
            _ => (ConsoleColor.White, "???")
        };

        var original = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write($"    [{label}]");
        Console.ForegroundColor = original;
        Console.Write($" {result.StepDescription} ({result.ElapsedMs}ms)");

        if (result.Message != null && (result.Outcome != StepOutcome.Passed || _verbose))
        {
            Console.Write($" - {result.Message}");
        }

        Console.WriteLine();
    }

    public void ReportScenarioEnd(ScenarioResult result)
    {
        var color = result.Passed ? ConsoleColor.Green : ConsoleColor.Red;
        var label = result.Passed ? "PASSED" : "FAILED";

        var original = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write($"  Result: {label}");
        Console.ForegroundColor = original;
        Console.WriteLine($" ({result.ElapsedMs}ms)");
    }

    public void ReportSuiteEnd(SuiteResult result)
    {
        Console.WriteLine();

        var passedCount = result.Scenarios.Count(s => s.Passed);
        var totalCount = result.Scenarios.Count;
        var color = result.Passed ? ConsoleColor.Green : ConsoleColor.Red;

        var original = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write($"Summary: {passedCount}/{totalCount} scenarios passed.");
        Console.ForegroundColor = original;
        Console.WriteLine($" Total time: {result.ElapsedMs}ms");
    }
}
