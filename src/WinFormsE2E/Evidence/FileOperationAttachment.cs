namespace WinFormsE2E.Evidence;

public class FileOperationAttachment : IEvidenceAttachment
{
    public string Name { get; }
    public string Type => "file-operation";
    public string? FilePath => null;
    public string? Content { get; }

    public string Operation { get; }
    public string TargetPath { get; }
    public bool FileExisted { get; }
    public bool FileExists { get; }
    public long? FileSize { get; }
    public bool Passed { get; }
    public string? ExpectedValue { get; }
    public string? ActualValue { get; }

    public FileOperationAttachment(
        string label,
        string operation,
        string targetPath,
        bool fileExisted,
        bool fileExists,
        long? fileSize = null,
        bool passed = true,
        string? expectedValue = null,
        string? actualValue = null)
    {
        Name = label;
        Operation = operation;
        TargetPath = targetPath;
        FileExisted = fileExisted;
        FileExists = fileExists;
        FileSize = fileSize;
        Passed = passed;
        ExpectedValue = expectedValue;
        ActualValue = actualValue;
        Content = BuildContentSummary(operation, targetPath, fileExists);
    }

    private static string BuildContentSummary(string operation, string path, bool exists)
    {
        var fileName = System.IO.Path.GetFileName(path);
        return operation switch
        {
            "assertFile" => $"{fileName}: exists={exists}",
            "deleteFile" => $"{fileName}: deleted",
            _ => $"{fileName}: {operation}"
        };
    }
}
