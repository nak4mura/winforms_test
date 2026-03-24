using System.Text.RegularExpressions;
using System.Windows.Automation;
using WinFormsE2E.Models;

namespace WinFormsE2E.Assertions;

public class AssertionEngine
{
    public (bool passed, string message) Assert(AutomationElement element, ExpectedValue expect)
    {
        var actual = GetPropertyValue(element, expect.Property);
        var passed = Evaluate(actual, expect.Operator, expect.Value);

        if (passed)
        {
            return (true, $"[{expect.Property}] {expect.Operator} \"{expect.Value}\" -> OK");
        }

        return (false, $"Expected [{expect.Property}] {expect.Operator} \"{expect.Value}\", but was \"{actual ?? "(null)"}\"");
    }

    public (bool passed, string message) AssertWindowTitle(AutomationElement window, ExpectedValue expect)
    {
        string? actual;
        try
        {
            actual = window.Current.Name;
        }
        catch (ElementNotAvailableException)
        {
            return (false, "Window is no longer available.");
        }

        var passed = Evaluate(actual, expect.Operator, expect.Value);

        if (passed)
        {
            return (true, $"Window title {expect.Operator} \"{expect.Value}\" -> OK");
        }

        return (false, $"Expected window title {expect.Operator} \"{expect.Value}\", but was \"{actual ?? "(null)"}\"");
    }

    private static bool Evaluate(string? actual, string op, string expected)
    {
        if (actual == null) return false;

        return op.ToLowerInvariant() switch
        {
            "equals" => actual == expected,
            "notequals" => actual != expected,
            "contains" => actual.Contains(expected, StringComparison.Ordinal),
            "startswith" => actual.StartsWith(expected, StringComparison.Ordinal),
            "endswith" => actual.EndsWith(expected, StringComparison.Ordinal),
            "matches" => Regex.IsMatch(actual, expected),
            "containsignorecase" => actual.Contains(expected, StringComparison.OrdinalIgnoreCase),
            "equalsignorecase" => actual.Equals(expected, StringComparison.OrdinalIgnoreCase),
            _ => throw new InvalidOperationException($"Unknown operator: {op}")
        };
    }

    private static string? GetPropertyValue(AutomationElement element, string property)
    {
        return property.ToLowerInvariant() switch
        {
            "name" => element.Current.Name,
            "automationid" => element.Current.AutomationId,
            "classname" => element.Current.ClassName,
            "isenabled" => element.Current.IsEnabled.ToString(),
            "isoffscreen" => element.Current.IsOffscreen.ToString(),
            "value" => GetValuePatternValue(element),
            "togglestate" => GetToggleState(element),
            "isselected" => GetSelectionItemState(element),
            "boundingrectangle" => element.Current.BoundingRectangle.ToString(),
            "itemstatus" => element.Current.ItemStatus,
            _ => throw new InvalidOperationException($"Unsupported property: {property}")
        };
    }

    private static string? GetValuePatternValue(AutomationElement element)
    {
        if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern))
        {
            return ((ValuePattern)pattern).Current.Value;
        }

        // For DataGridView and other grid controls, collect values from all descendant cells
        var controlType = element.Current.ControlType;
        if (controlType == ControlType.DataGrid || controlType == ControlType.Table || controlType == ControlType.List)
        {
            return GetGridCellValues(element);
        }

        return null;
    }

    private static string? GetGridCellValues(AutomationElement gridElement)
    {
        var values = new List<string>();
        var condition = new PropertyCondition(AutomationElement.IsValuePatternAvailableProperty, true);
        var cells = gridElement.FindAll(TreeScope.Descendants, condition);

        foreach (AutomationElement cell in cells)
        {
            if (cell.TryGetCurrentPattern(ValuePattern.Pattern, out var cellPattern))
            {
                var val = ((ValuePattern)cellPattern).Current.Value;
                if (!string.IsNullOrEmpty(val))
                {
                    values.Add(val);
                }
            }
        }

        return values.Count > 0 ? string.Join(" | ", values) : null;
    }

    private static string? GetToggleState(AutomationElement element)
    {
        if (element.TryGetCurrentPattern(TogglePattern.Pattern, out var pattern))
        {
            return ((TogglePattern)pattern).Current.ToggleState.ToString();
        }
        return null;
    }

    private static string? GetSelectionItemState(AutomationElement element)
    {
        if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern))
        {
            return ((SelectionItemPattern)pattern).Current.IsSelected.ToString();
        }
        return null;
    }
}
