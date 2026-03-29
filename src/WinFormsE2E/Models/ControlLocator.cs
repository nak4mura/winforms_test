using System.Text.Json.Serialization;

namespace WinFormsE2E.Models;

public class ControlLocator
{
    [JsonPropertyName("automationId")]
    public string? AutomationId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("className")]
    public string? ClassName { get; set; }

    [JsonPropertyName("controlType")]
    public string? ControlType { get; set; }

    [JsonPropertyName("treePath")]
    public int[]? TreePath { get; set; }

    [JsonPropertyName("index")]
    public int? Index { get; set; }

    public string DisplayName
    {
        get
        {
            if (AutomationId != null) return $"[AutomationId={AutomationId}]";
            if (Name != null) return $"[Name={Name}]";
            if (ClassName != null) return $"[ClassName={ClassName}]";
            if (TreePath != null) return $"[TreePath={string.Join(",", TreePath)}]";
            return "[unknown]";
        }
    }
}
