namespace WinFormsE2E.Evidence;

public class StepExecutionContext
{
    public IntPtr WindowHandle { get; set; }
    public string? WindowTitle { get; set; }
    public string? StepDescription { get; set; }
    public string? Action { get; set; }
}
