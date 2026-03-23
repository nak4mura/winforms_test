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
            var windows = AutomationElement.RootElement.FindAll(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window));

            foreach (AutomationElement window in windows)
            {
                try
                {
                    var name = window.Current.Name;
                    if (name != null && MatchesTitle(name, titlePattern))
                        return window;
                }
                catch (ElementNotAvailableException)
                {
                    // Window disappeared during enumeration
                }
            }
            return null;
        }, timeoutMs, retryIntervalMs);
    }

    public bool WaitForTitleChange(AutomationElement window, string expectedTitle, int timeoutMs, int retryIntervalMs = 200)
    {
        return WaitStrategy.WaitUntilTrue(() =>
        {
            try
            {
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
        throw new InvalidOperationException("Window does not support WindowPattern.");
    }

    private static bool MatchesTitle(string actual, string pattern)
    {
        if (pattern == actual) return true;

        if (pattern.Contains('*') || pattern.Contains('?'))
        {
            var regex = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            return Regex.IsMatch(actual, regex, RegexOptions.IgnoreCase);
        }

        return actual.Contains(pattern, StringComparison.OrdinalIgnoreCase);
    }
}
