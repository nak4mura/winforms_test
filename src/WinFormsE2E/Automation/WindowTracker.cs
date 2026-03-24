using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Automation;
using WinFormsE2E.Core;

namespace WinFormsE2E.Automation;

public class WindowTracker
{
    public AutomationElement? WaitForWindow(string titlePattern, int timeoutMs, int retryIntervalMs = 200)
    {
        return WaitStrategy.WaitUntil(() =>
        {
            // Use Win32 EnumWindows to find windows. This avoids UIAutomation's FindAll
            // which can hang when there's a pending InvokePattern.Invoke() call
            // (e.g., when a button opens a modal dialog).
            var targetHwnd = FindWindowByTitle(titlePattern);
            if (targetHwnd == IntPtr.Zero)
                return null;

            try
            {
                return AutomationElement.FromHandle(targetHwnd);
            }
            catch
            {
                return null;
            }
        }, timeoutMs, retryIntervalMs);
    }

    public bool WaitForTitleChange(AutomationElement window, string expectedTitle, int timeoutMs, int retryIntervalMs = 200)
    {
        return WaitStrategy.WaitUntilTrue(() =>
        {
            try
            {
                var hwnd = new IntPtr(window.Current.NativeWindowHandle);
                if (hwnd != IntPtr.Zero)
                {
                    var title = GetWindowTitle(hwnd);
                    return title != null && MatchesTitle(title, expectedTitle);
                }

                var name = window.Current.Name;
                return name != null && MatchesTitle(name, expectedTitle);
            }
            catch (ElementNotAvailableException)
            {
                return false;
            }
        }, timeoutMs, retryIntervalMs);
    }

    public AutomationElement? WaitForModalDialog(AutomationElement parent, int timeoutMs, int retryIntervalMs = 200)
    {
        return WaitStrategy.WaitUntil(() =>
        {
            var dialogs = parent.FindAll(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window));

            foreach (AutomationElement dialog in dialogs)
            {
                try
                {
                    if (dialog.Current.IsEnabled)
                        return dialog;
                }
                catch (ElementNotAvailableException)
                {
                }
            }
            return null;
        }, timeoutMs, retryIntervalMs);
    }

    public void CloseWindow(AutomationElement window)
    {
        if (window.TryGetCurrentPattern(WindowPattern.Pattern, out var pattern))
        {
            ((WindowPattern)pattern).Close();
            return;
        }

        // Fallback: use Win32 WM_CLOSE
        var hwnd = new IntPtr(window.Current.NativeWindowHandle);
        if (hwnd != IntPtr.Zero)
        {
            PostMessage(hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            return;
        }

        throw new InvalidOperationException("Window does not support WindowPattern and has no native handle.");
    }

    private IntPtr FindWindowByTitle(string titlePattern)
    {
        IntPtr found = IntPtr.Zero;

        EnumWindows((hwnd, _) =>
        {
            if (!IsWindowVisible(hwnd))
                return true; // continue enumeration

            var title = GetWindowTitle(hwnd);
            if (title != null && MatchesTitle(title, titlePattern))
            {
                found = hwnd;
                return false; // stop enumeration
            }
            return true;
        }, IntPtr.Zero);

        return found;
    }

    private static string? GetWindowTitle(IntPtr hwnd)
    {
        int length = GetWindowTextLength(hwnd);
        if (length == 0) return null;

        var sb = new StringBuilder(length + 1);
        GetWindowText(hwnd, sb, sb.Capacity);
        return sb.ToString();
    }

    internal static bool MatchesTitle(string actual, string pattern)
    {
        if (pattern == actual) return true;

        if (pattern.Contains('*') || pattern.Contains('?'))
        {
            var regex = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            return Regex.IsMatch(actual, regex, RegexOptions.IgnoreCase);
        }

        return actual.Contains(pattern, StringComparison.OrdinalIgnoreCase);
    }

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    private const uint WM_CLOSE = 0x0010;

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
}
