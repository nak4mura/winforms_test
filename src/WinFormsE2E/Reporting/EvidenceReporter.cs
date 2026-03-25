using WinFormsE2E.Evidence;
using WinFormsE2E.Models;

namespace WinFormsE2E.Reporting;

public class EvidenceReporter : IResultReporter
{
    private readonly EvidenceBundle _bundle;
    private readonly string _outputDir;

    public EvidenceReporter(EvidenceBundle bundle, string outputDir)
    {
        _bundle = bundle;
        _outputDir = outputDir;
    }

    public void ReportScenarioStart(string scenarioName) { }
    public void ReportStepResult(StepResult result) { }
    public void ReportScenarioEnd(ScenarioResult result) { }

    public void ReportSuiteEnd(SuiteResult result)
    {
        var generator = new HtmlReportGenerator(_outputDir);
        generator.Generate(_bundle, result);
        Console.WriteLine($"Evidence report: {System.IO.Path.GetFullPath(System.IO.Path.Combine(_outputDir, "report.html"))}");
    }
}
