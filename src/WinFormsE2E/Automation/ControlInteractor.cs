using System.Runtime.InteropServices;
using System.Windows.Automation;
using System.Windows.Forms;

namespace WinFormsE2E.Automation;

public class ControlInteractor
{
    public void Click(AutomationElement element)
    {
        // For controls with a native window handle (buttons, etc.), use focus + Space key.
        // This avoids InvokePattern.Invoke() which blocks on modal dialogs and causes
        // all subsequent UIAutomation COM calls to hang (including EnumWindows + FromHandle).
        var hwnd = new IntPtr(element.Current.NativeWindowHandle);
        if (hwnd != IntPtr.Zero)
        {
            SetForegroundWindow(hwnd);
            element.SetFocus();
            Thread.Sleep(50);
            SendKeys.SendWait(" ");
            return;
        }

        // For controls without a native handle (e.g., ToolStripMenuItems), use InvokePattern
        if (element.TryGetCurrentPattern(InvokePattern.Pattern, out var pattern))
        {
            ((InvokePattern)pattern).Invoke();
            return;
        }

        if (element.TryGetCurrentPattern(TogglePattern.Pattern, out var togglePattern))
        {
            ((TogglePattern)togglePattern).Toggle();
            return;
        }

        ClickByPoint(element);
    }

    public void DoubleClick(AutomationElement element)
    {
        var rect = element.Current.BoundingRectangle;
        if (rect.IsEmpty) throw new InvalidOperationException("Control has no bounding rectangle for double click.");

        var x = (int)(rect.X + rect.Width / 2);
        var y = (int)(rect.Y + rect.Height / 2);

        SetCursorPos(x, y);
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
    }

    public void Type(AutomationElement element, string text)
    {
        if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern))
        {
            ((ValuePattern)pattern).SetValue(text);
            return;
        }

        element.SetFocus();
        Thread.Sleep(50);
        SendKeys.SendWait(text);
    }

    public void SendKeysToElement(AutomationElement element, string keys)
    {
        element.SetFocus();
        Thread.Sleep(50);
        SendKeys.SendWait(keys);
    }

    public void Select(AutomationElement element)
    {
        if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern))
        {
            ((SelectionItemPattern)pattern).Select();
            return;
        }
        throw new InvalidOperationException("Control does not support SelectionItemPattern.");
    }

    public void Toggle(AutomationElement element)
    {
        if (element.TryGetCurrentPattern(TogglePattern.Pattern, out var pattern))
        {
            ((TogglePattern)pattern).Toggle();
            return;
        }
        throw new InvalidOperationException("Control does not support TogglePattern.");
    }

    public void ExpandCollapse(AutomationElement element, bool expand)
    {
        if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var pattern))
        {
            var ecp = (ExpandCollapsePattern)pattern;
            if (expand) ecp.Expand(); else ecp.Collapse();
            return;
        }
        throw new InvalidOperationException("Control does not support ExpandCollapsePattern.");
    }

    private void ClickByPoint(AutomationElement element)
    {
        var rect = element.Current.BoundingRectangle;
        if (rect.IsEmpty) throw new InvalidOperationException("Control has no bounding rectangle for click.");

        var x = (int)(rect.X + rect.Width / 2);
        var y = (int)(rect.Y + rect.Height / 2);

        SetCursorPos(x, y);
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
    }

    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
}
