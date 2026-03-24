# CLAUDE.md — WinForms E2E Test Framework

This document describes the codebase structure, development conventions, and workflows for AI assistants working on this repository.

---

## Project Overview

This is a **WinForms E2E (End-to-End) test automation framework** for testing Windows Forms (.NET) applications via Windows UI Automation. It consists of:

1. **`src/WinFormsE2E/`** — CLI test runner that executes JSON-defined test suites against a running WinForms app
2. **`src/TestApp/`** — A sample WinForms application used to validate the test runner itself
3. **`tests/testapp/`** — JSON test suite files targeting `TestApp`

---

## Repository Branch Strategy

```
main          ← stable releases only (currently just the initial commit)
develop       ← integration branch; all feature PRs target here
feature/xxx   ← individual feature branches
```

- **Always develop on `feature/` branches** cut from `develop`
- **PRs always target `develop`**, never `main`
- `main` is merged from `develop` at stable release points

---

## Directory Structure

```
winforms_test/
├── src/
│   ├── WinFormsE2E/               # Main CLI test runner
│   │   ├── Program.cs             # Entry point; CLI argument parsing
│   │   ├── Core/
│   │   │   ├── StepExecutor.cs    # Dispatches test step actions
│   │   │   ├── TestRunner.cs      # Orchestrates suite/scenario execution
│   │   │   └── TestContext.cs     # Runtime state (current window, process, etc.)
│   │   ├── Automation/
│   │   │   ├── ControlInteractor.cs  # Low-level UI Automation interactions
│   │   │   └── WindowTracker.cs      # Window discovery and switching
│   │   ├── Assertions/
│   │   │   └── AssertionEngine.cs    # Evaluates assert/assertWindow steps
│   │   ├── Models/                   # TestSuite, TestScenario, TestStep, StepResult, etc.
│   │   ├── Reporting/
│   │   │   ├── ConsoleReporter.cs
│   │   │   ├── JsonReporter.cs
│   │   │   └── EvidenceReporter.cs   # HTML evidence report emitter
│   │   └── Evidence/                 # Screenshot capture & evidence collection
│   │       ├── IEvidenceCollector.cs
│   │       ├── IEvidenceAttachment.cs
│   │       ├── IDbEvidenceProvider.cs  # Interface for future DB evidence
│   │       ├── ScreenshotCollector.cs  # Implements IEvidenceCollector
│   │       ├── ScreenshotCapture.cs    # Win32 window screenshot via GDI
│   │       ├── HtmlReportGenerator.cs  # Generates report.html
│   │       ├── EvidenceBundle.cs       # Data model for collected evidence
│   │       └── StepExecutionContext.cs # Per-step context passed to collector
│   └── TestApp/                    # Sample WinForms app under test
│       └── Forms/
│           ├── MainForm.cs         # Main window with menu navigation
│           ├── CrudForm.cs         # DataGridView CRUD screen
│           ├── MessageForm.cs      # MessageBox / dialog conditional flow
│           ├── FunctionKeyForm.cs  # F1/F5/F10/Esc key bindings
│           └── InputControlForm.cs # Input validation (half-width, IME, numeric, etc.)
├── tests/
│   └── testapp/                    # JSON test suites for TestApp
│       ├── 00_inspect.json         # UI tree inspection (debugging aid)
│       ├── 01_navigation.json      # Screen navigation tests
│       ├── 02_crud.json            # CRUD operation tests
│       ├── 03_message.json         # MessageBox conditional branch tests
│       ├── 04_functionkey.json     # Function key tests
│       └── 05_input_control.json   # Input restriction tests
└── README.md
```

---

## Build & Run

### Prerequisites
- .NET 9 SDK
- Windows (Windows UI Automation and WinForms are Windows-only)

### Build
```bash
dotnet build
```

### Run a test suite
```bash
# Basic
dotnet run --project src/WinFormsE2E -- tests/testapp/01_navigation.json

# With verbose console output
dotnet run --project src/WinFormsE2E -- tests/testapp/01_navigation.json --verbose

# Save results as JSON
dotnet run --project src/WinFormsE2E -- tests/testapp/01_navigation.json --output results.json

# Enable evidence (screenshots + HTML report, auto-named output dir)
dotnet run --project src/WinFormsE2E -- tests/testapp/01_navigation.json --evidence

# Evidence with explicit output directory
dotnet run --project src/WinFormsE2E -- tests/testapp/01_navigation.json --evidence-output evidence/run1
```

The test runner exits with code `0` (all passed) or `1` (failures). Code `2` indicates a configuration/startup error.

---

## Test Suite JSON Format

```json
{
  "suite": "Suite display name",
  "application": {
    "path": "src\\TestApp\\bin\\Debug\\net9.0-windows\\TestApp.exe",
    "startArgs": "",
    "windowTitle": "テストアプリケーション",
    "startupWaitMs": 3000
  },
  "settings": {
    "defaultTimeoutMs": 5000,
    "retryIntervalMs": 200
  },
  "scenarios": [
    {
      "name": "Scenario name",
      "steps": [
        {
          "action": "click",
          "target": { "automationId": "BtnAdd" },
          "description": "Human-readable description"
        }
      ]
    }
  ]
}
```

### Target Selectors
Elements are located by one or more of:
| Field | Description |
|---|---|
| `name` | Matches `AutomationElement.Current.Name` (control text or explicit Name) |
| `automationId` | Matches `AutomationElement.Current.AutomationId` — set via the WinForms `Name` property |

### Supported Actions
| Action | Description |
|---|---|
| `click` | Click a UI element |
| `type` | Type text into a focused text box (`value` field) |
| `clear` | Clear a text box |
| `select` | Select an item in a list or combo box by name |
| `assert` | Assert an element's value/text against `expect` |
| `assertWindow` | Assert the current window's properties against `expect` |
| `waitForWindow` | Wait for a window with matching `windowTitle` to appear |
| `switchWindow` | Switch current context to a window matching `windowTitle` |
| `closeWindow` | Close the current window |
| `wait` | Pause execution for `ms` milliseconds |
| `inspect` | Dump the UI Automation tree to console (debugging; uses `ms` as max depth) |

**Pending (see open issues):**
- `expandcollapse` — Open/close a combo box drop-down (Issue #10)

### Assertion `expect` Object
```json
{
  "property": "Name",
  "operator": "contains",
  "value": "expected text"
}
```
Operators: `equals`, `contains`, `startsWith`, `endsWith`, `notEquals`

---

## Key Architectural Conventions

### TestApp: Control Naming for AutomationId
Every interactable control in `TestApp` **must** have its WinForms `Name` property set to a descriptive PascalCase identifier. This `Name` becomes the `AutomationId` used in test JSON.

```csharp
// Good — has a Name for test targeting
_btnAdd = new Button { Name = "BtnAdd", Text = "追加(&A)", ... };

// Bad — no Name set, can only be targeted by Text
_btnAdd = new Button { Text = "追加(&A)", ... };
```

Naming conventions:
- Buttons: `BtnXxx` (e.g., `BtnAdd`, `BtnUpdate`, `BtnCrudBack`)
- TextBoxes: `TxtXxx` (e.g., `TxtItemName`, `TxtItemDescription`)
- DataGridView: `XxxDataGrid` (e.g., `CrudDataGrid`)
- Forms: `XxxForm` (e.g., `CrudForm`, `MainForm`)

### StepExecutor: Adding New Actions
Actions are dispatched via a `switch` expression in `StepExecutor.Execute()`. To add a new action:
1. Add a case to the `switch` in `Execute()`:
   ```csharp
   "expandcollapse" => ExecuteExpandCollapse(step, context),
   ```
2. Implement the private method `ExecuteExpandCollapse(TestStep step, TestContext context)`.
3. Return `StepResult.Pass(...)` on success or throw an exception (caught by the outer try/catch which returns `StepResult.Err(...)`).

### Evidence Collection: IEvidenceCollector
`IEvidenceCollector` hooks into `TestRunner` and `StepExecutor`. It is optional (`null` by default). Implementations must handle exceptions gracefully — all calls from production code are wrapped in `try/catch` or `SafeCall()`.

The `ScreenshotCollector` is the current implementation; `IDbEvidenceProvider` is an interface stub for future DB result capture (Issue #8).

### Scenario Isolation
After each scenario, `TestRunner.ResetToMainWindow()` automatically closes any non-main windows and returns focus to the main window. This ensures scenarios do not bleed into each other.

---

## Commit Message Convention

```
<type>: #<issue> Brief description in Japanese or English

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>
```

Types: `feat`, `fix`, `test`, `chore`, `docs`, `refactor`

Examples:
```
feat: #3 データCRUD画面の実装
fix: #6 WinForms入力制御の実装
test: TestApp用E2Eテストケースを作成
chore: add bin/obj to .gitignore
```

---

## Open Issues (as of 2026-03-24)

| # | Title | Summary |
|---|---|---|
| [#8](https://github.com/nak4mura/winforms_test/issues/8) | DB期待値確認 | Add DB query result assertion. Execute a SQL, compare result rows against expected values in JSON. `IDbEvidenceProvider` interface is already defined. |
| [#10](https://github.com/nak4mura/winforms_test/issues/10) | コンボボックスのテスト入力対応 | Add `expandcollapse` action to `StepExecutor`; add a ComboBox screen to TestApp; add sample JSON in `tests/samples/`. `ControlInteractor.ExpandCollapse()` already exists. |

---

## Open Pull Requests (as of 2026-03-24)

| # | Title | Branch | Base |
|---|---|---|---|
| [#18](https://github.com/nak4mura/winforms_test/pull/18) | エビデンス作成機能の実装 | `feature/evidence-creation` | `develop` |

---

## Platform Notes

- This project **only runs on Windows**. UI Automation (`System.Windows.Automation`) and WinForms are Windows-only APIs.
- The test runner launches the target application as a child process and controls it via COM-based UI Automation.
- `ControlInteractor.Click()` uses `SetForegroundWindow` + `SendKeys.SendWait(" ")` rather than `InvokePattern.Invoke()` for controls with a native HWND, to avoid modal dialog deadlocks in UI Automation.
- Screenshots in `ScreenshotCapture` use Win32 `GetWindowRect` + `Graphics.CopyFromScreen` (GDI), not `PrintWindow`, so the window must be visible and not minimized.
