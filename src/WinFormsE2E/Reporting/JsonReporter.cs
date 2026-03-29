using System.Text.Json;
using WinFormsE2E.Models;

namespace WinFormsE2E.Reporting;

public class JsonReporter : IResultReporter
{
    private readonly string _outputPath;
    private SuiteResult? _suiteResult;

    public JsonReporter(string outputPath)
    {
        _outputPath = outputPath;
    }

    public void ReportScenarioStart(string scenarioName) { }

    public void ReportStepResult(StepResult result) { }

    public void ReportScenarioEnd(ScenarioResult result) { }

    public void ReportSuiteEnd(SuiteResult result)
    {
        _suiteResult = result;
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var json = JsonSerializer.Serialize(result, options);
        System.IO.File.WriteAllText(_outputPath, json);
        Console.WriteLine($"Results written to: {_outputPath}");
    }
}
