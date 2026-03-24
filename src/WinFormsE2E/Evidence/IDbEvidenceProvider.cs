namespace WinFormsE2E.Evidence;

/// <summary>
/// DB確認結果をエビデンスとして記録するためのプロバイダーインターフェース。
///
/// 使用例:
/// <code>
/// var attachment = dbProvider.CaptureQueryResult("MainDB", "SELECT * FROM Users WHERE Id = 1", "ユーザー登録確認");
/// stepEvidence.Attachments.Add(attachment);
/// </code>
/// </summary>
public interface IDbEvidenceProvider
{
    /// <summary>
    /// 指定されたクエリを実行し、結果をエビデンスとして記録する。
    /// </summary>
    /// <param name="connectionName">接続先の識別名</param>
    /// <param name="query">実行するSQLクエリ</param>
    /// <param name="label">エビデンスの表示ラベル</param>
    /// <returns>クエリ結果を含むエビデンス添付データ</returns>
    IEvidenceAttachment CaptureQueryResult(string connectionName, string query, string label);
}
