namespace WinFormsE2E.Evidence;

public interface IEvidenceAttachment
{
    string Name { get; }
    string Type { get; }
    string? FilePath { get; }
    string? Content { get; }
}
