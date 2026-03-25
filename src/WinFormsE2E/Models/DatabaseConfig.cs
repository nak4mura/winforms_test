using System.Text.Json.Serialization;

namespace WinFormsE2E.Models;

public class DatabaseConfig
{
    [JsonPropertyName("connections")]
    public Dictionary<string, DbConnectionInfo> Connections { get; set; } = new();
}

public class DbConnectionInfo
{
    [JsonPropertyName("provider")]
    public string Provider { get; set; } = "";

    [JsonPropertyName("connectionString")]
    public string ConnectionString { get; set; } = "";
}

public class DbQuery
{
    [JsonPropertyName("connectionName")]
    public string ConnectionName { get; set; } = "";

    [JsonPropertyName("sql")]
    public string Sql { get; set; } = "";
}
