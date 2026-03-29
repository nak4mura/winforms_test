using System.Diagnostics;
using System.Text.Json;
using System.Windows.Automation;
using WinFormsE2E.Assertions;
using WinFormsE2E.Automation;
using WinFormsE2E.Evidence;
using WinFormsE2E.Models;

namespace WinFormsE2E.Core;

public class StepExecutor
{
    private readonly ControlFinder _finder = new();
    private readonly ControlInteractor _interactor = new();
    private readonly WindowTracker _windowTracker = new();
    private readonly AssertionEngine _assertionEngine = new();

    public StepResult Execute(TestStep step, TestContext context, IEvidenceCollector? collector = null)
    {
        var sw = Stopwatch.StartNew();
        var execContext = BuildExecutionContext(step, context);
        try
        {
            SafeCall(() => collector?.OnBeforeStep(execContext));

            var result = step.Action.ToLowerInvariant() switch
            {
                "click" => ExecuteClick(step, context),
                "doubleclick" => ExecuteDoubleClick(step, context),
                "type" => ExecuteType(step, context),
                "sendkeys" => ExecuteSendKeys(step, context),
                "select" => ExecuteSelect(step, context),
                "toggle" => ExecuteToggle(step, context),
                "assert" => ExecuteAssert(step, context),
                "assertwindow" => ExecuteAssertWindow(step, context),
                "waitforwindow" => ExecuteWaitForWindow(step, context),
                "waitforcontrol" => ExecuteWaitForControl(step, context),
                "switchwindow" => ExecuteSwitchWindow(step, context),
                "closewindow" => ExecuteCloseWindow(step, context),
                "wait" => ExecuteWait(step),
                "inspect" => ExecuteInspect(step, context),
                "assertdb" => ExecuteAssertDb(step, context, collector),
                "executedb" => ExecuteExecuteDb(step, context),
                "expandcollapse" => ExecuteExpandCollapse(step, context),
                "assertfile" => ExecuteAssertFile(step),
                "deletefile" => ExecuteDeleteFile(step),
                _ => throw new InvalidOperationException($"Unknown action: {step.Action}")
            };

            result.ElapsedMs = sw.ElapsedMilliseconds;
            SafeCall(() => collector?.OnAfterStep(BuildExecutionContext(step, context)));
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            SafeCall(() => collector?.OnAfterStep(BuildExecutionContext(step, context)));
            return StepResult.Err(step.DisplayName, ex.Message, sw.ElapsedMilliseconds);
        }
    }

    private static StepExecutionContext BuildExecutionContext(TestStep step, TestContext context)
    {
        var hwnd = IntPtr.Zero;
        string? title = null;

        if (context.CurrentWindow != null)
        {
            try
            {
                hwnd = new IntPtr(context.CurrentWindow.Current.NativeWindowHandle);
                title = context.CurrentWindow.Current.Name;
            }
            catch { }
        }

        return new StepExecutionContext
        {
            WindowHandle = hwnd,
            WindowTitle = title,
            StepDescription = step.DisplayName,
            Action = step.Action
        };
    }

    private static void SafeCall(Action action)
    {
        try { action(); }
        catch { }
    }

    private StepResult ExecuteClick(TestStep step, TestContext context)
    {
        var element = FindElement(step, context);
        _interactor.Click(element);
        return StepResult.Pass(step.DisplayName, 0);
    }

    private StepResult ExecuteDoubleClick(TestStep step, TestContext context)
    {
        var element = FindElement(step, context);
        _interactor.DoubleClick(element);
        return StepResult.Pass(step.DisplayName, 0);
    }

    private StepResult ExecuteType(TestStep step, TestContext context)
    {
        if (step.Text == null) throw new InvalidOperationException("'type' action requires 'text' field.");
        var element = FindElement(step, context);
        _interactor.Type(element, step.Text);
        return StepResult.Pass(step.DisplayName, 0);
    }

    private StepResult ExecuteSendKeys(TestStep step, TestContext context)
    {
        if (step.Keys == null) throw new InvalidOperationException("'sendKeys' action requires 'keys' field.");

        if (step.Target != null)
        {
            var element = FindElement(step, context);
            _interactor.SendKeysToElement(element, step.Keys);
        }
        else
        {
            System.Windows.Forms.SendKeys.SendWait(step.Keys);
        }

        return StepResult.Pass(step.DisplayName, 0);
    }

    private StepResult ExecuteSelect(TestStep step, TestContext context)
    {
        if (step.Value == null) throw new InvalidOperationException("'select' action requires 'value' field.");
        var parent = FindElement(step, context);

        // Find the item to select within the parent control
        var itemLocator = new ControlLocator { Name = step.Value };
        var timeout = step.TimeoutMs ?? context.Settings.DefaultTimeoutMs;
        var item = _finder.Find(parent, itemLocator, timeout, context.Settings.RetryIntervalMs);
        if (item == null) throw new InvalidOperationException($"Selection item not found: {step.Value}");

        _interactor.Select(item);
        return StepResult.Pass(step.DisplayName, 0);
    }

    private StepResult ExecuteToggle(TestStep step, TestContext context)
    {
        var element = FindElement(step, context);
        _interactor.Toggle(element);
        return StepResult.Pass(step.DisplayName, 0);
    }

    private StepResult ExecuteExpandCollapse(TestStep step, TestContext context)
    {
        var element = FindElement(step, context);
        var expand = !string.Equals(step.Value, "collapse", StringComparison.OrdinalIgnoreCase);
        _interactor.ExpandCollapse(element, expand);
        return StepResult.Pass(step.DisplayName, 0);
    }

    private StepResult ExecuteAssert(TestStep step, TestContext context)
    {
        if (step.Expect == null) throw new InvalidOperationException("'assert' action requires 'expect' field.");
        var element = FindElement(step, context);
        var (passed, message) = _assertionEngine.Assert(element, step.Expect);

        return passed
            ? StepResult.Pass(step.DisplayName, 0)
            : StepResult.Fail(step.DisplayName, message, 0);
    }

    private StepResult ExecuteAssertWindow(TestStep step, TestContext context)
    {
        if (step.Expect == null) throw new InvalidOperationException("'assertWindow' action requires 'expect' field.");
        if (context.CurrentWindow == null) throw new InvalidOperationException("No current window.");

        var (passed, message) = _assertionEngine.AssertWindowTitle(context.CurrentWindow, step.Expect);

        return passed
            ? StepResult.Pass(step.DisplayName, 0)
            : StepResult.Fail(step.DisplayName, message, 0);
    }

    private StepResult ExecuteWaitForWindow(TestStep step, TestContext context)
    {
        if (step.WindowTitle == null) throw new InvalidOperationException("'waitForWindow' action requires 'windowTitle' field.");
        var timeout = step.TimeoutMs ?? context.Settings.DefaultTimeoutMs;
        var window = _windowTracker.WaitForWindow(step.WindowTitle, timeout, context.Settings.RetryIntervalMs,
            processId: context.AppProcess?.Id);

        if (window == null)
        {
            return StepResult.Fail(step.DisplayName, $"Window not found: {step.WindowTitle}", 0);
        }

        context.CurrentWindow = window;
        return StepResult.Pass(step.DisplayName, 0);
    }

    private StepResult ExecuteWaitForControl(TestStep step, TestContext context)
    {
        var element = FindElementOrNull(step, context);
        if (element == null)
        {
            return StepResult.Fail(step.DisplayName, $"Control not found: {step.Target?.DisplayName}", 0);
        }
        return StepResult.Pass(step.DisplayName, 0);
    }

    private StepResult ExecuteSwitchWindow(TestStep step, TestContext context)
    {
        if (step.WindowTitle == null) throw new InvalidOperationException("'switchWindow' action requires 'windowTitle' field.");
        var timeout = step.TimeoutMs ?? context.Settings.DefaultTimeoutMs;
        var window = _windowTracker.WaitForWindow(step.WindowTitle, timeout, context.Settings.RetryIntervalMs,
            processId: context.AppProcess?.Id);

        if (window == null)
        {
            return StepResult.Fail(step.DisplayName, $"Window not found: {step.WindowTitle}", 0);
        }

        context.CurrentWindow = window;

        try { window.SetFocus(); } catch { }

        return StepResult.Pass(step.DisplayName, 0);
    }

    private StepResult ExecuteCloseWindow(TestStep step, TestContext context)
    {
        if (step.WindowTitle != null)
        {
            var timeout = step.TimeoutMs ?? context.Settings.DefaultTimeoutMs;
            var window = _windowTracker.WaitForWindow(step.WindowTitle, timeout, context.Settings.RetryIntervalMs,
                processId: context.AppProcess?.Id);
            if (window != null)
            {
                _windowTracker.CloseWindow(window);
            }
        }
        else if (context.CurrentWindow != null)
        {
            _windowTracker.CloseWindow(context.CurrentWindow);
            context.CurrentWindow = null;
        }

        return StepResult.Pass(step.DisplayName, 0);
    }

    private StepResult ExecuteWait(TestStep step)
    {
        var ms = step.Ms ?? 1000;
        Thread.Sleep(ms);
        return StepResult.Pass(step.DisplayName, 0);
    }

    private StepResult ExecuteInspect(TestStep step, TestContext context)
    {
        if (context.CurrentWindow == null) throw new InvalidOperationException("No current window.");
        var maxDepth = step.Ms ?? 5; // reuse 'ms' field as max depth
        Console.WriteLine("=== UI Tree Inspection ===");
        DumpElement(context.CurrentWindow, 0, (int)maxDepth);
        Console.WriteLine("=== End Inspection ===");
        return StepResult.Pass(step.DisplayName, 0);
    }

    private static void DumpElement(AutomationElement element, int depth, int maxDepth)
    {
        if (depth > maxDepth) return;
        var indent = new string(' ', depth * 2);
        try
        {
            var current = element.Current;
            Console.WriteLine($"{indent}[{current.ControlType.ProgrammaticName}] " +
                $"Name=\"{current.Name}\" " +
                $"ClassName=\"{current.ClassName}\" " +
                $"AutomationId=\"{current.AutomationId}\"");
        }
        catch (ElementNotAvailableException)
        {
            Console.WriteLine($"{indent}[unavailable]");
            return;
        }

        try
        {
            var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);
            foreach (AutomationElement child in children)
            {
                DumpElement(child, depth + 1, maxDepth);
            }
        }
        catch (ElementNotAvailableException) { }
    }

    private StepResult ExecuteExecuteDb(TestStep step, TestContext context)
    {
        if (context.DbManager == null)
            throw new InvalidOperationException("'executeDb' requires 'database' configuration in the test suite.");
        if (step.Query == null)
            throw new InvalidOperationException("'executeDb' action requires 'query' field.");

        var affected = context.DbManager.ExecuteNonQuery(step.Query.ConnectionName, step.Query.Sql);
        return StepResult.Pass(step.DisplayName, 0);
    }

    private StepResult ExecuteAssertDb(TestStep step, TestContext context, IEvidenceCollector? collector)
    {
        if (context.DbManager == null)
            throw new InvalidOperationException("'assertDb' requires 'database' configuration in the test suite.");
        if (step.Query == null)
            throw new InvalidOperationException("'assertDb' action requires 'query' field.");
        if (step.ExpectedRows == null)
            throw new InvalidOperationException("'assertDb' action requires 'expectedRows' field.");

        var result = context.DbManager.ExecuteQuery(step.Query.ConnectionName, step.Query.Sql);

        int? failedRowIdx = null;
        int? failedColIdx = null;
        string? failMessage = null;

        if (result.Rows.Count != step.ExpectedRows.Count)
        {
            failMessage = $"行数が一致しません。期待: {step.ExpectedRows.Count} 行, 実際: {result.Rows.Count} 行";
        }
        else
        {
            for (int rowIdx = 0; rowIdx < step.ExpectedRows.Count; rowIdx++)
            {
                var expectedRow = step.ExpectedRows[rowIdx];
                var actualRow = result.Rows[rowIdx];

                var colCount = Math.Min(expectedRow.Count, actualRow.Count);
                for (int colIdx = 0; colIdx < colCount; colIdx++)
                {
                    var expected = expectedRow[colIdx];
                    var actual = actualRow[colIdx];
                    var columnName = colIdx < result.Columns.Count ? result.Columns[colIdx] : $"Column{colIdx}";

                    if (!CompareDbValues(expected, actual))
                    {
                        failedRowIdx = rowIdx;
                        failedColIdx = colIdx;
                        failMessage = $"行 {rowIdx + 1}, カラム '{columnName}': 期待値 = {FormatJsonElement(expected)}, 実際値 = {FormatActualValue(actual)}";
                        break;
                    }
                }

                if (failMessage != null) break;

                if (expectedRow.Count != actualRow.Count)
                {
                    failedRowIdx = rowIdx;
                    failMessage = $"行 {rowIdx + 1}: カラム数が一致しません。期待: {expectedRow.Count}, 実際: {actualRow.Count}";
                    break;
                }
            }
        }

        // Attach DB evidence if collector is available
        SafeCall(() =>
        {
            if (collector is ScreenshotCollector sc)
            {
                var attachment = new DbQueryAttachment(
                    step.Description ?? "DB Query",
                    step.Query.ConnectionName,
                    step.Query.Sql,
                    result,
                    step.ExpectedRows,
                    failedRowIdx,
                    failedColIdx);
                sc.AttachToCurrentStep(attachment);
            }
        });

        if (failMessage != null)
            return StepResult.Fail(step.DisplayName, failMessage, 0);

        return StepResult.Pass(step.DisplayName, 0);
    }

    private static bool CompareDbValues(JsonElement expected, object? actual)
    {
        if (expected.ValueKind == JsonValueKind.Null)
            return actual == null || actual == DBNull.Value;

        if (actual == null || actual == DBNull.Value)
            return false;

        return expected.ValueKind switch
        {
            JsonValueKind.String => expected.GetString() == actual.ToString(),
            JsonValueKind.Number => CompareNumeric(expected, actual),
            JsonValueKind.True => actual is bool b1 && b1,
            JsonValueKind.False => actual is bool b2 && !b2,
            _ => expected.ToString() == actual.ToString()
        };
    }

    private static bool CompareNumeric(JsonElement expected, object actual)
    {
        try
        {
            var expectedDecimal = expected.GetDecimal();
            var actualDecimal = Convert.ToDecimal(actual);
            return expectedDecimal == actualDecimal;
        }
        catch
        {
            return expected.ToString() == actual.ToString();
        }
    }

    private static string FormatJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Null => "null",
            JsonValueKind.String => $"\"{element.GetString()}\"",
            _ => element.ToString()
        };
    }

    private static string FormatActualValue(object? value)
    {
        if (value == null || value == DBNull.Value) return "null";
        if (value is string s) return $"\"{s}\"";
        return value.ToString() ?? "null";
    }

    private static StepResult ExecuteAssertFile(TestStep step)
    {
        if (step.Value == null) throw new InvalidOperationException("'assertFile' action requires 'value' field (file path).");
        if (step.Expect == null) throw new InvalidOperationException("'assertFile' action requires 'expect' field.");

        var filePath = Environment.ExpandEnvironmentVariables(step.Value);

        var actual = step.Expect.Property.ToLowerInvariant() switch
        {
            "exists" => System.IO.File.Exists(filePath).ToString(),
            _ => throw new InvalidOperationException($"Unsupported assertFile property: {step.Expect.Property}")
        };

        var passed = step.Expect.Operator.ToLowerInvariant() switch
        {
            "equals" or "equalsignorecase" => string.Equals(actual, step.Expect.Value, StringComparison.OrdinalIgnoreCase),
            "notequals" => !string.Equals(actual, step.Expect.Value, StringComparison.OrdinalIgnoreCase),
            _ => string.Equals(actual, step.Expect.Value, StringComparison.OrdinalIgnoreCase)
        };

        return passed
            ? StepResult.Pass(step.DisplayName, 0)
            : StepResult.Fail(step.DisplayName, $"Expected file [{step.Expect.Property}] {step.Expect.Operator} \"{step.Expect.Value}\", but was \"{actual}\" (path: {filePath})", 0);
    }

    private static StepResult ExecuteDeleteFile(TestStep step)
    {
        if (step.Value == null) throw new InvalidOperationException("'deleteFile' action requires 'value' field (file path).");

        var filePath = Environment.ExpandEnvironmentVariables(step.Value);

        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        return StepResult.Pass(step.DisplayName, 0);
    }

    private AutomationElement FindElement(TestStep step, TestContext context)
    {
        var element = FindElementOrNull(step, context);
        if (element == null)
        {
            throw new InvalidOperationException($"Control not found: {step.Target?.DisplayName ?? "(no target)"}");
        }
        return element;
    }

    private AutomationElement? FindElementOrNull(TestStep step, TestContext context)
    {
        if (step.Target == null) throw new InvalidOperationException($"Action '{step.Action}' requires 'target' field.");
        if (context.CurrentWindow == null) throw new InvalidOperationException("No current window.");

        var timeout = step.TimeoutMs ?? context.Settings.DefaultTimeoutMs;
        return _finder.Find(context.CurrentWindow, step.Target, timeout, context.Settings.RetryIntervalMs);
    }
}
