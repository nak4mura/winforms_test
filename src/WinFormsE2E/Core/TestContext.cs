using System.Diagnostics;
using System.Windows.Automation;
using WinFormsE2E.Models;

namespace WinFormsE2E.Core;

public class TestContext
{
    public Process? AppProcess { get; set; }
    public AutomationElement? CurrentWindow { get; set; }
    public AutomationElement Desktop => AutomationElement.RootElement;
    public TestSettings Settings { get; set; } = new();
    public bool Verbose { get; set; }
}
