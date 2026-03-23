using System.Diagnostics;
using WinFormsE2E.Automation;
using WinFormsE2E.Models;
using WinFormsE2E.Reporting;

namespace WinFormsE2E.Core;

public class TestRunner
{
    private readonly TestSuite _suite;
    private readonly List<IResultReporter> _reporters;
    private readonly StepExecutor _stepExecutor = new();
    private readonly WindowTracker _windowTracker = new();

    public TestRunner(TestSuite suite, List<IResultReporter> reporters)
    {
        _suite = suite;
        _reporters = reporters;
    }

    public SuiteResult Run()
    {
        var suiteStopwatch = Stopwatch.StartNew();
        var suiteResult = new SuiteResult { SuiteName = _suite.Suite };

        Console.WriteLine($"Suite: {_suite.Suite}");

        var context = new TestContext { Settings = _suite.Settings };
        Process? process = null;

        try
        {
            process = LaunchApplication(_suite.Application);
            context.AppProcess = process;

            var window = _windowTracker.WaitForWindow(
                _suite.Application.WindowTitle,
                _suite.Application.StartupWaitMs,
                _suite.Settings.RetryIntervalMs);

            if (window == null)
            {
                Console.Error.WriteLine($"Failed to find application window: {_suite.Application.WindowTitle}");
                suiteResult.ElapsedMs = suiteStopwatch.ElapsedMilliseconds;
                return suiteResult;
            }

            context.CurrentWindow = window;

            foreach (var scenario in _suite.Scenarios)
            {
                var scenarioResult = RunScenario(scenario, context);
                suiteResult.Scenarios.Add(scenarioResult);
            }
        }
        finally
        {
            TryCloseProcess(process);
            suiteStopwatch.Stop();
            suiteResult.ElapsedMs = suiteStopwatch.ElapsedMilliseconds;
        }

        foreach (var reporter in _reporters)
        {
            reporter.ReportSuiteEnd(suiteResult);
        }

        return suiteResult;
    }

    private ScenarioResult RunScenario(TestScenario scenario, TestContext context)
    {
        var scenarioStopwatch = Stopwatch.StartNew();
        var scenarioResult = new ScenarioResult { ScenarioName = scenario.Name };

        foreach (var reporter in _reporters)
        {
            reporter.ReportScenarioStart(scenario.Name);
        }

        var failed = false;

        foreach (var step in scenario.Steps)
        {
            StepResult stepResult;

            if (failed)
            {
                stepResult = new StepResult
                {
                    StepDescription = step.DisplayName,
                    Outcome = StepOutcome.Skipped,
                    Message = "Skipped due to previous failure"
                };
            }
            else
            {
                stepResult = _stepExecutor.Execute(step, context);
            }

            scenarioResult.Steps.Add(stepResult);

            foreach (var reporter in _reporters)
            {
                reporter.ReportStepResult(stepResult);
            }

            if (stepResult.Outcome == StepOutcome.Failed || stepResult.Outcome == StepOutcome.Error)
            {
                failed = true;
            }
        }

        scenarioStopwatch.Stop();
        scenarioResult.ElapsedMs = scenarioStopwatch.ElapsedMilliseconds;

        foreach (var reporter in _reporters)
        {
            reporter.ReportScenarioEnd(scenarioResult);
        }

        return scenarioResult;
    }

    private static Process? LaunchApplication(ApplicationConfig config)
    {
        if (string.IsNullOrEmpty(config.Path)) return null;

        var startInfo = new ProcessStartInfo
        {
            FileName = config.Path,
            Arguments = config.StartArgs,
            UseShellExecute = true
        };

        return Process.Start(startInfo);
    }

    private static void TryCloseProcess(Process? process)
    {
        if (process == null || process.HasExited) return;

        try
        {
            process.CloseMainWindow();
            if (!process.WaitForExit(3000))
            {
                process.Kill();
            }
        }
        catch
        {
            // Best effort cleanup
        }
    }
}
