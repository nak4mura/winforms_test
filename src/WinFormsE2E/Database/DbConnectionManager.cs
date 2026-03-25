using System.Data.Common;
using Microsoft.Data.SqlClient;
using WinFormsE2E.Models;

namespace WinFormsE2E.Database;

public class DbConnectionManager : IDisposable
{
    private readonly Dictionary<string, DbConnectionInfo> _connectionInfos;
    private readonly Dictionary<string, DbConnection> _connections = new();
    private bool _disposed;

    public DbConnectionManager(DatabaseConfig config)
    {
        _connectionInfos = config.Connections;
    }

    public DbQueryResult ExecuteQuery(string connectionName, string sql)
    {
        var connection = GetOrCreateConnection(connectionName);
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        using var reader = command.ExecuteReader();
        var result = new DbQueryResult();

        for (int i = 0; i < reader.FieldCount; i++)
        {
            result.Columns.Add(reader.GetName(i));
        }

        while (reader.Read())
        {
            var row = new List<object?>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row.Add(reader.IsDBNull(i) ? null : reader.GetValue(i));
            }
            result.Rows.Add(row);
        }

        return result;
    }

    private DbConnection GetOrCreateConnection(string connectionName)
    {
        if (_connections.TryGetValue(connectionName, out var existing))
        {
            if (existing.State == System.Data.ConnectionState.Open)
                return existing;

            existing.Dispose();
            _connections.Remove(connectionName);
        }

        if (!_connectionInfos.TryGetValue(connectionName, out var info))
        {
            throw new InvalidOperationException(
                $"接続名 '{connectionName}' は database.connections に定義されていません。" +
                $"定義済みの接続名: [{string.Join(", ", _connectionInfos.Keys)}]");
        }

        var connection = CreateConnection(info);
        try
        {
            connection.Open();
        }
        catch (Exception ex)
        {
            connection.Dispose();
            throw new InvalidOperationException(
                $"接続名 '{connectionName}' (Provider: {info.Provider}) への接続に失敗しました: {ex.Message}", ex);
        }

        _connections[connectionName] = connection;
        return connection;
    }

    private static DbConnection CreateConnection(DbConnectionInfo info)
    {
        return info.Provider.ToLowerInvariant() switch
        {
            "sqlserver" or "mssql" => new SqlConnection(info.ConnectionString),
            _ => throw new NotSupportedException(
                $"プロバイダー '{info.Provider}' はサポートされていません。サポート対象: sqlserver, mssql")
        };
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var connection in _connections.Values)
        {
            try { connection.Dispose(); }
            catch { /* クリーンアップ中の例外は無視 */ }
        }
        _connections.Clear();
    }
}
