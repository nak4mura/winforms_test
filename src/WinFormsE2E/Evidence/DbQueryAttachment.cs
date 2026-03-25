using System.Text.Json;
using WinFormsE2E.Database;

namespace WinFormsE2E.Evidence;

public class DbQueryAttachment : IEvidenceAttachment
{
    public string Name { get; }
    public string Type => "db-query";
    public string? FilePath => null;
    public string? Content { get; }

    public string Sql { get; }
    public string ConnectionName { get; }
    public List<string> Columns { get; }
    public List<List<object?>> Rows { get; }
    public List<List<JsonElement>>? ExpectedRows { get; }
    public int? FailedRowIndex { get; }
    public int? FailedColIndex { get; }

    public DbQueryAttachment(
        string label,
        string connectionName,
        string sql,
        DbQueryResult queryResult,
        List<List<JsonElement>>? expectedRows = null,
        int? failedRowIndex = null,
        int? failedColIndex = null)
    {
        Name = label;
        ConnectionName = connectionName;
        Sql = sql;
        Columns = queryResult.Columns;
        Rows = queryResult.Rows;
        ExpectedRows = expectedRows;
        FailedRowIndex = failedRowIndex;
        FailedColIndex = failedColIndex;
        Content = BuildContentSummary(queryResult);
    }

    private static string BuildContentSummary(DbQueryResult result)
    {
        return $"{result.Columns.Count} columns, {result.Rows.Count} rows";
    }
}
