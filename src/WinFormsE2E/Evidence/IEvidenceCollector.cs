namespace WinFormsE2E.Evidence;

public interface IEvidenceCollector
{
    void OnScenarioStart(string scenarioName);
    void OnBeforeStep(StepExecutionContext context);
    void OnAfterStep(StepExecutionContext context);
    void OnScenarioEnd();
    EvidenceBundle GetBundle();
}
