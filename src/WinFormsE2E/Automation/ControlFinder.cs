using System.Windows.Automation;
using WinFormsE2E.Core;
using WinFormsE2E.Models;

namespace WinFormsE2E.Automation;

public class ControlFinder
{
    public AutomationElement? Find(AutomationElement parent, ControlLocator locator, int timeoutMs, int retryIntervalMs = 200)
    {
        if (locator.TreePath != null)
        {
            return FindByTreePath(parent, locator.TreePath, timeoutMs, retryIntervalMs);
        }

        return WaitStrategy.WaitUntil(() => FindImmediate(parent, locator), timeoutMs, retryIntervalMs);
    }

    private AutomationElement? FindImmediate(AutomationElement parent, ControlLocator locator)
    {
        var condition = BuildCondition(locator);
        if (condition == null) return null;

        var elements = parent.FindAll(TreeScope.Descendants, condition);
        if (elements.Count == 0) return null;

        var index = locator.Index ?? 0;
        if (index < elements.Count)
        {
            return elements[index];
        }

        return null;
    }

    private Condition? BuildCondition(ControlLocator locator)
    {
        var conditions = new List<Condition>();

        if (locator.AutomationId != null)
        {
            conditions.Add(new PropertyCondition(AutomationElement.AutomationIdProperty, locator.AutomationId));
        }

        if (locator.Name != null)
        {
            conditions.Add(new PropertyCondition(AutomationElement.NameProperty, locator.Name));
        }

        if (locator.ClassName != null)
        {
            conditions.Add(new PropertyCondition(AutomationElement.ClassNameProperty, locator.ClassName));
        }

        if (locator.ControlType != null)
        {
            var ct = MapControlType(locator.ControlType);
            if (ct != null)
            {
                conditions.Add(new PropertyCondition(AutomationElement.ControlTypeProperty, ct));
            }
        }

        return conditions.Count switch
        {
            0 => null,
            1 => conditions[0],
            _ => new AndCondition(conditions.ToArray())
        };
    }

    private AutomationElement? FindByTreePath(AutomationElement parent, int[] treePath, int timeoutMs, int retryIntervalMs)
    {
        return WaitStrategy.WaitUntil(() =>
        {
            var current = parent;
            foreach (var childIndex in treePath)
            {
                var walker = TreeWalker.ControlViewWalker;
                var child = walker.GetFirstChild(current);
                for (var i = 0; i < childIndex && child != null; i++)
                {
                    child = walker.GetNextSibling(child);
                }
                if (child == null) return null;
                current = child;
            }
            return current;
        }, timeoutMs, retryIntervalMs);
    }

    private static ControlType? MapControlType(string typeName)
    {
        return typeName.ToLowerInvariant() switch
        {
            "button" => ControlType.Button,
            "checkbox" or "check box" => ControlType.CheckBox,
            "combobox" or "combo box" => ControlType.ComboBox,
            "edit" => ControlType.Edit,
            "hyperlink" => ControlType.Hyperlink,
            "image" => ControlType.Image,
            "list" => ControlType.List,
            "listitem" or "list item" => ControlType.ListItem,
            "menu" => ControlType.Menu,
            "menuitem" or "menu item" => ControlType.MenuItem,
            "progressbar" or "progress bar" => ControlType.ProgressBar,
            "radiobutton" or "radio button" => ControlType.RadioButton,
            "scrollbar" or "scroll bar" => ControlType.ScrollBar,
            "slider" => ControlType.Slider,
            "spinner" => ControlType.Spinner,
            "statusbar" or "status bar" => ControlType.StatusBar,
            "tab" => ControlType.Tab,
            "tabitem" or "tab item" => ControlType.TabItem,
            "text" => ControlType.Text,
            "toolbar" or "tool bar" => ControlType.ToolBar,
            "tooltip" or "tool tip" => ControlType.ToolTip,
            "tree" => ControlType.Tree,
            "treeitem" or "tree item" => ControlType.TreeItem,
            "window" => ControlType.Window,
            "datagrid" or "data grid" => ControlType.DataGrid,
            "dataitem" or "data item" => ControlType.DataItem,
            "document" => ControlType.Document,
            "group" => ControlType.Group,
            "header" => ControlType.Header,
            "headeritem" or "header item" => ControlType.HeaderItem,
            "pane" => ControlType.Pane,
            "table" => ControlType.Table,
            "titlebar" or "title bar" => ControlType.TitleBar,
            _ => null
        };
    }
}
