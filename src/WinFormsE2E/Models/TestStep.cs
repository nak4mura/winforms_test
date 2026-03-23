using System.Text.Json.Serialization;

namespace WinFormsE2E.Models;

public class TestStep
{
    [JsonPropertyName("action")]
    public string Action { get; set; } = "";

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("target")]
    public ControlLocator? Target { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("keys")]
    public string? Keys { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("expect")]
    public ExpectedValue? Expect { get; set; }

    [JsonPropertyName("windowTitle")]
    public string? WindowTitle { get; set; }

    [JsonPropertyName("timeoutMs")]
    public int? TimeoutMs { get; set; }

    [JsonPropertyName("ms")]
    public int? Ms { get; set; }

    public string DisplayName => Description ?? $"{Action} {Target?.DisplayName ?? WindowTitle ?? ""}".Trim();
}

public class ExpectedValue
{
    [JsonPropertyName("property")]
    public string Property { get; set; } = "";

    [JsonPropertyName("operator")]
    public string Operator { get; set; } = "equals";

    [JsonPropertyName("value")]
    public string Value { get; set; } = "";
}
