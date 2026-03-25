using System.Text;
using WinFormsE2E.Models;

namespace WinFormsE2E.Evidence;

public class HtmlReportGenerator
{
    private readonly string _outputDir;

    public HtmlReportGenerator(string outputDir)
    {
        _outputDir = outputDir;
    }

    public void Generate(EvidenceBundle bundle, SuiteResult suiteResult)
    {
        var reportPath = System.IO.Path.Combine(_outputDir, "report.html");
        var reportDir = System.IO.Path.GetDirectoryName(reportPath);
        if (!string.IsNullOrEmpty(reportDir))
            System.IO.Directory.CreateDirectory(reportDir);

        var html = BuildHtml(bundle, suiteResult);
        System.IO.File.WriteAllText(reportPath, html, Encoding.UTF8);
    }

    private string BuildHtml(EvidenceBundle bundle, SuiteResult suiteResult)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"ja\">");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"UTF-8\">");
        sb.AppendLine($"<title>E2Eテストエビデンス - {Escape(suiteResult.SuiteName)}</title>");
        sb.AppendLine("<style>");
        sb.AppendLine(GetCss());
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        // Suite header
        var suiteStatus = suiteResult.Passed ? "passed" : "failed";
        sb.AppendLine($"<div class=\"suite-header {suiteStatus}\">");
        sb.AppendLine($"<h1>{Escape(suiteResult.SuiteName)}</h1>");
        sb.AppendLine($"<p>結果: {(suiteResult.Passed ? "PASS" : "FAIL")} | 実行時間: {suiteResult.ElapsedMs}ms</p>");
        sb.AppendLine("</div>");

        // Scenarios
        for (int i = 0; i < bundle.Scenarios.Count; i++)
        {
            var scenarioEvidence = bundle.Scenarios[i];
            var scenarioResult = i < suiteResult.Scenarios.Count ? suiteResult.Scenarios[i] : null;
            var scenarioStatus = scenarioResult?.Passed == true ? "passed" : "failed";

            sb.AppendLine($"<div class=\"scenario\">");
            sb.AppendLine($"<h2 class=\"{scenarioStatus}\">{Escape(scenarioEvidence.ScenarioName)}</h2>");

            for (int j = 0; j < scenarioEvidence.Steps.Count; j++)
            {
                var stepEvidence = scenarioEvidence.Steps[j];
                var stepResult = scenarioResult != null && j < scenarioResult.Steps.Count ? scenarioResult.Steps[j] : null;
                var outcomeClass = GetOutcomeClass(stepResult?.Outcome);

                sb.AppendLine($"<div class=\"step-card {outcomeClass}\">");
                sb.AppendLine($"<div class=\"step-header\">");
                sb.AppendLine($"<span class=\"step-number\">Step {j + 1}</span>");
                sb.AppendLine($"<span class=\"step-desc\">{Escape(stepEvidence.StepDescription)}</span>");
                if (stepResult != null)
                    sb.AppendLine($"<span class=\"step-outcome {outcomeClass}\">{stepResult.Outcome}</span>");
                sb.AppendLine("</div>");

                if (stepEvidence.Action != null)
                    sb.AppendLine($"<div class=\"step-action\">操作: {Escape(stepEvidence.Action)}</div>");

                sb.AppendLine("<div class=\"screenshots\">");

                if (stepEvidence.BeforeScreenshotPath != null)
                {
                    var relativeBefore = GetRelativePath(stepEvidence.BeforeScreenshotPath);
                    sb.AppendLine("<div class=\"screenshot\">");
                    sb.AppendLine("<div class=\"screenshot-label\">操作前</div>");
                    sb.AppendLine($"<a href=\"{Escape(relativeBefore)}\" class=\"lightbox-trigger\">");
                    sb.AppendLine($"<img src=\"{Escape(relativeBefore)}\" alt=\"操作前\">");
                    sb.AppendLine("</a>");
                    sb.AppendLine("</div>");
                }

                if (stepEvidence.AfterScreenshotPath != null)
                {
                    var relativeAfter = GetRelativePath(stepEvidence.AfterScreenshotPath);
                    sb.AppendLine("<div class=\"screenshot\">");
                    sb.AppendLine("<div class=\"screenshot-label\">操作後</div>");
                    sb.AppendLine($"<a href=\"{Escape(relativeAfter)}\" class=\"lightbox-trigger\">");
                    sb.AppendLine($"<img src=\"{Escape(relativeAfter)}\" alt=\"操作後\">");
                    sb.AppendLine("</a>");
                    sb.AppendLine("</div>");
                }

                sb.AppendLine("</div>"); // screenshots

                if (stepResult?.Message != null)
                    sb.AppendLine($"<div class=\"step-message\">{Escape(stepResult.Message)}</div>");

                sb.AppendLine("</div>"); // step-card
            }

            sb.AppendLine("</div>"); // scenario
        }

        // Lightbox overlay
        sb.AppendLine("<div id=\"lightbox\" class=\"lightbox\" onclick=\"this.style.display='none'\">");
        sb.AppendLine("<img id=\"lightbox-img\" src=\"\" alt=\"拡大表示\">");
        sb.AppendLine("</div>");

        sb.AppendLine("<script>");
        sb.AppendLine(GetScript());
        sb.AppendLine("</script>");

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private string GetRelativePath(string absolutePath)
    {
        try
        {
            var reportDir = System.IO.Path.GetFullPath(_outputDir);
            var filePath = System.IO.Path.GetFullPath(absolutePath);
            return System.IO.Path.GetRelativePath(reportDir, filePath).Replace('\\', '/');
        }
        catch
        {
            return absolutePath.Replace('\\', '/');
        }
    }

    private static string GetOutcomeClass(StepOutcome? outcome) => outcome switch
    {
        StepOutcome.Passed => "pass",
        StepOutcome.Failed => "fail",
        StepOutcome.Error => "error",
        StepOutcome.Skipped => "skip",
        _ => ""
    };

    private static string Escape(string text) =>
        System.Net.WebUtility.HtmlEncode(text);

    private static string GetCss() => """
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { font-family: 'Segoe UI', Tahoma, sans-serif; background: #f5f5f5; padding: 20px; }
        .suite-header { padding: 20px; border-radius: 8px; margin-bottom: 20px; color: #fff; }
        .suite-header.passed { background: #2e7d32; }
        .suite-header.failed { background: #c62828; }
        .suite-header h1 { font-size: 1.5em; }
        .scenario { background: #fff; border-radius: 8px; padding: 20px; margin-bottom: 16px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }
        .scenario h2 { font-size: 1.2em; margin-bottom: 12px; padding-bottom: 8px; border-bottom: 2px solid #eee; }
        .scenario h2.passed { border-bottom-color: #4caf50; }
        .scenario h2.failed { border-bottom-color: #f44336; }
        .step-card { border: 1px solid #e0e0e0; border-radius: 6px; padding: 12px; margin-bottom: 12px; border-left: 4px solid #9e9e9e; }
        .step-card.pass { border-left-color: #4caf50; }
        .step-card.fail { border-left-color: #f44336; }
        .step-card.error { border-left-color: #ff9800; }
        .step-card.skip { border-left-color: #9e9e9e; }
        .step-header { display: flex; align-items: center; gap: 12px; margin-bottom: 8px; }
        .step-number { font-weight: bold; color: #666; }
        .step-desc { flex: 1; }
        .step-outcome { padding: 2px 8px; border-radius: 4px; font-size: 0.85em; font-weight: bold; color: #fff; }
        .step-outcome.pass { background: #4caf50; }
        .step-outcome.fail { background: #f44336; }
        .step-outcome.error { background: #ff9800; }
        .step-outcome.skip { background: #9e9e9e; }
        .step-action { color: #555; margin-bottom: 8px; font-size: 0.9em; }
        .step-message { color: #c62828; font-size: 0.85em; margin-top: 8px; padding: 8px; background: #fff3f3; border-radius: 4px; }
        .screenshots { display: flex; gap: 16px; }
        .screenshot { flex: 1; }
        .screenshot-label { font-size: 0.8em; color: #888; margin-bottom: 4px; text-align: center; }
        .screenshot img { width: 100%; border: 1px solid #ddd; border-radius: 4px; cursor: pointer; }
        .lightbox { display: none; position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.85); z-index: 1000; cursor: pointer; justify-content: center; align-items: center; }
        .lightbox img { max-width: 95%; max-height: 95%; object-fit: contain; }
        """;

    private static string GetScript() => """
        document.querySelectorAll('.lightbox-trigger').forEach(function(a) {
            a.addEventListener('click', function(e) {
                e.preventDefault();
                var lb = document.getElementById('lightbox');
                document.getElementById('lightbox-img').src = this.href;
                lb.style.display = 'flex';
            });
        });
        """;
}
