namespace WinFormsE2E.Evidence;

public class ScreenshotCollector : IEvidenceCollector
{
    private readonly string _outputDir;
    private readonly ScreenshotCapture _capture = new();
    private readonly EvidenceBundle _bundle = new();
    private ScenarioEvidence? _currentScenario;
    private StepEvidence? _currentStep;
    private int _scenarioIndex;
    private int _stepIndex;

    public ScreenshotCollector(string outputDir)
    {
        _outputDir = outputDir;
    }

    public void OnScenarioStart(string scenarioName)
    {
        _currentScenario = new ScenarioEvidence { ScenarioName = scenarioName };
        _bundle.Scenarios.Add(_currentScenario);
        _stepIndex = 0;
        _scenarioIndex++;
    }

    public void OnBeforeStep(StepExecutionContext context)
    {
        _stepIndex++;
        _currentStep = new StepEvidence
        {
            StepDescription = context.StepDescription ?? "",
            Action = context.Action
        };

        var path = GetScreenshotPath("before");
        CaptureAndSave(context.WindowHandle, path);
        _currentStep.BeforeScreenshotPath = path;
    }

    public void OnAfterStep(StepExecutionContext context)
    {
        if (_currentStep == null) return;

        var path = GetScreenshotPath("after");
        CaptureAndSave(context.WindowHandle, path);
        _currentStep.AfterScreenshotPath = path;

        _currentScenario?.Steps.Add(_currentStep);
        _currentStep = null;
    }

    public void AttachToCurrentStep(IEvidenceAttachment attachment)
    {
        _currentStep?.Attachments.Add(attachment);
    }

    public void OnScenarioEnd()
    {
        _currentScenario = null;
    }

    public EvidenceBundle GetBundle()
    {
        return _bundle;
    }

    private string GetScreenshotPath(string timing)
    {
        var scenarioDir = $"{_scenarioIndex:D2}_{SanitizeName(_currentScenario?.ScenarioName ?? "unknown")}";
        return System.IO.Path.Combine(_outputDir, "screenshots", scenarioDir, $"{_stepIndex:D3}_{timing}.png");
    }

    private void CaptureAndSave(IntPtr hwnd, string path)
    {
        var bitmap = _capture.CaptureWindow(hwnd);
        if (bitmap != null)
        {
            _capture.SaveScreenshot(bitmap, path);
            bitmap.Dispose();
        }
    }

    private static string SanitizeName(string name)
    {
        var sanitized = name.Replace(" ", "_");
        foreach (var c in System.IO.Path.GetInvalidFileNameChars())
        {
            sanitized = sanitized.Replace(c, '_');
        }
        return sanitized;
    }
}
