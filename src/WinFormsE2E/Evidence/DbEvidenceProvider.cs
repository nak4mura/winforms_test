using System.Text.Json;
using WinFormsE2E.Database;

namespace WinFormsE2E.Evidence;

public class DbEvidenceProvider : IDbEvidenceProvider
{
    private readonly DbConnectionManager _dbManager;

    public DbEvidenceProvider(DbConnectionManager dbManager)
    {
        _dbManager = dbManager;
    }

    public IEvidenceAttachment CaptureQueryResult(string connectionName, string query, string label)
    {
        var result = _dbManager.ExecuteQuery(connectionName, query);
        return new DbQueryAttachment(label, connectionName, query, result);
    }

    public DbQueryAttachment CaptureQueryResultWithExpected(
        string connectionName,
        string sql,
        string label,
        DbQueryResult queryResult,
        List<List<JsonElement>>? expectedRows,
        int? failedRowIndex = null,
        int? failedColIndex = null)
    {
        return new DbQueryAttachment(label, connectionName, sql, queryResult, expectedRows, failedRowIndex, failedColIndex);
    }
}
