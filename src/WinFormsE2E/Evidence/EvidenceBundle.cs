namespace WinFormsE2E.Evidence;

public class EvidenceBundle
{
    public List<ScenarioEvidence> Scenarios { get; } = new();
}

public class ScenarioEvidence
{
    public string ScenarioName { get; set; } = "";
    public List<StepEvidence> Steps { get; } = new();
}

public class StepEvidence
{
    public string StepDescription { get; set; } = "";
    public string? Action { get; set; }
    public string? BeforeScreenshotPath { get; set; }
    public string? AfterScreenshotPath { get; set; }
    public List<IEvidenceAttachment> Attachments { get; } = new();
}
