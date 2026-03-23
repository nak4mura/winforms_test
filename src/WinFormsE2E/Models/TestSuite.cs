using System.Text.Json.Serialization;

namespace WinFormsE2E.Models;

public class TestSuite
{
    [JsonPropertyName("suite")]
    public string Suite { get; set; } = "";

    [JsonPropertyName("application")]
    public ApplicationConfig Application { get; set; } = new();

    [JsonPropertyName("settings")]
    public TestSettings Settings { get; set; } = new();

    [JsonPropertyName("scenarios")]
    public List<TestScenario> Scenarios { get; set; } = new();
}

public class ApplicationConfig
{
    [JsonPropertyName("path")]
    public string Path { get; set; } = "";

    [JsonPropertyName("startArgs")]
    public string StartArgs { get; set; } = "";

    [JsonPropertyName("windowTitle")]
    public string WindowTitle { get; set; } = "";

    [JsonPropertyName("startupWaitMs")]
    public int StartupWaitMs { get; set; } = 3000;
}

public class TestSettings
{
    [JsonPropertyName("defaultTimeoutMs")]
    public int DefaultTimeoutMs { get; set; } = 5000;

    [JsonPropertyName("retryIntervalMs")]
    public int RetryIntervalMs { get; set; } = 200;
}
