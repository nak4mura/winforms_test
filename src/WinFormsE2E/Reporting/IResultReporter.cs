using WinFormsE2E.Models;

namespace WinFormsE2E.Reporting;

public interface IResultReporter
{
    void ReportScenarioStart(string scenarioName);
    void ReportStepResult(StepResult result);
    void ReportScenarioEnd(ScenarioResult result);
    void ReportSuiteEnd(SuiteResult result);
}
