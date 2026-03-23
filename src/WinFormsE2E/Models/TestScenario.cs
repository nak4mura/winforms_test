using System.Text.Json.Serialization;

namespace WinFormsE2E.Models;

public class TestScenario
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("steps")]
    public List<TestStep> Steps { get; set; } = new();
}
