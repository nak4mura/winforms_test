using System.Text.Json.Serialization;

namespace WinFormsE2E.Models;

public enum StepOutcome
{
    Passed,
    Failed,
    Error,
    Skipped
}

public class StepResult
{
    [JsonPropertyName("step")]
    public string StepDescription { get; set; } = "";

    [JsonPropertyName("outcome")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public StepOutcome Outcome { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("elapsedMs")]
    public long ElapsedMs { get; set; }

    public static StepResult Pass(string description, long elapsedMs) => new()
    {
        StepDescription = description,
        Outcome = StepOutcome.Passed,
        ElapsedMs = elapsedMs
    };

    public static StepResult Fail(string description, string message, long elapsedMs) => new()
    {
        StepDescription = description,
        Outcome = StepOutcome.Failed,
        Message = message,
        ElapsedMs = elapsedMs
    };

    public static StepResult Err(string description, string message, long elapsedMs) => new()
    {
        StepDescription = description,
        Outcome = StepOutcome.Error,
        Message = message,
        ElapsedMs = elapsedMs
    };
}

public class ScenarioResult
{
    [JsonPropertyName("scenario")]
    public string ScenarioName { get; set; } = "";

    [JsonPropertyName("steps")]
    public List<StepResult> Steps { get; set; } = new();

    [JsonPropertyName("passed")]
    public bool Passed => Steps.All(s => s.Outcome == StepOutcome.Passed);

    [JsonPropertyName("elapsedMs")]
    public long ElapsedMs { get; set; }
}

public class SuiteResult
{
    [JsonPropertyName("suite")]
    public string SuiteName { get; set; } = "";

    [JsonPropertyName("scenarios")]
    public List<ScenarioResult> Scenarios { get; set; } = new();

    [JsonPropertyName("passed")]
    public bool Passed => Scenarios.All(s => s.Passed);

    [JsonPropertyName("elapsedMs")]
    public long ElapsedMs { get; set; }
}
