namespace WinFormsE2E.Database;

public class DbQueryResult
{
    public List<string> Columns { get; set; } = new();
    public List<List<object?>> Rows { get; set; } = new();
}
