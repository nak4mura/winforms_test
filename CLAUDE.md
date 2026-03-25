# CLAUDE.md — WinForms E2E テストフレームワーク

このドキュメントは、AIアシスタントがこのリポジトリで作業する際に必要なコードベース構造、開発規約、ワークフローを説明します。

---

## プロジェクト概要

Windows UI Automation を用いて Windows Forms (.NET) アプリケーションをテストする **WinForms E2E（エンドツーエンド）テスト自動化フレームワーク**です。以下の3要素で構成されます。

1. **`src/WinFormsE2E/`** — JSONで定義されたテストスイートを実行するCLIテストランナー
2. **`src/TestApp/`** — テストランナー自身の動作確認用サンプル WinForms アプリケーション
3. **`tests/testapp/`** — `TestApp` を対象とした JSON テストスイートファイル群

---

## ブランチ戦略

```
main          ← 安定リリースのみ（現時点では初回コミットのみ）
develop       ← 統合ブランチ。全ての feature PR はここをターゲットにする
feature/xxx   ← 個別の機能ブランチ
```

- **常に `develop` から切った `feature/` ブランチで開発する**
- **PR のターゲットは常に `develop`**（`main` には直接 PR しない）
- `main` は安定リリース時のみ `develop` からマージする

---

## ディレクトリ構造

```
winforms_test/
├── src/
│   ├── WinFormsE2E/               # メイン CLI テストランナー
│   │   ├── Program.cs             # エントリポイント。CLIオプション解析
│   │   ├── Core/
│   │   │   ├── StepExecutor.cs    # テストステップのアクションをディスパッチ
│   │   │   ├── TestRunner.cs      # スイート／シナリオ実行のオーケストレーション
│   │   │   └── TestContext.cs     # 実行時状態（現在のウィンドウ、プロセス等）
│   │   ├── Automation/
│   │   │   ├── ControlInteractor.cs  # UI Automation 低レベル操作
│   │   │   └── WindowTracker.cs      # ウィンドウの検索と切り替え
│   │   ├── Assertions/
│   │   │   └── AssertionEngine.cs    # assert/assertWindow ステップの評価
│   │   ├── Models/                   # TestSuite, TestScenario, TestStep, StepResult 等
│   │   ├── Reporting/
│   │   │   ├── ConsoleReporter.cs
│   │   │   ├── JsonReporter.cs
│   │   │   └── EvidenceReporter.cs   # HTML エビデンスレポート出力
│   │   └── Evidence/                 # スクリーンショット取得・エビデンス収集
│   │       ├── IEvidenceCollector.cs
│   │       ├── IEvidenceAttachment.cs
│   │       ├── IDbEvidenceProvider.cs  # 将来の DB エビデンス用インターフェース
│   │       ├── ScreenshotCollector.cs  # IEvidenceCollector の実装
│   │       ├── ScreenshotCapture.cs    # GDI による Win32 ウィンドウスクリーンショット
│   │       ├── HtmlReportGenerator.cs  # report.html の生成
│   │       ├── EvidenceBundle.cs       # 収集エビデンスのデータモデル
│   │       └── StepExecutionContext.cs # コレクターに渡すステップ単位コンテキスト
│   └── TestApp/                    # テスト対象のサンプル WinForms アプリ
│       └── Forms/
│           ├── MainForm.cs         # メニューによる画面遷移を持つメインウィンドウ
│           ├── CrudForm.cs         # DataGridView による CRUD 画面
│           ├── MessageForm.cs      # MessageBox・ダイアログの条件分岐
│           ├── FunctionKeyForm.cs  # F1/F5/F10/Esc キーバインド
│           └── InputControlForm.cs # 入力制御（半角英数、IME、数値のみ等）
├── tests/
│   └── testapp/                    # TestApp 用 JSON テストスイート
│       ├── 00_inspect.json         # UI ツリーインスペクション（デバッグ用）
│       ├── 01_navigation.json      # 画面遷移テスト
│       ├── 02_crud.json            # CRUD 操作テスト
│       ├── 03_message.json         # MessageBox 条件分岐テスト
│       ├── 04_functionkey.json     # ファンクションキーテスト
│       └── 05_input_control.json   # 入力制御テスト
└── README.md
```

---

## ビルド & 実行

### 前提条件
- .NET 9 SDK
- Windows（Windows UI Automation および WinForms は Windows 専用 API）

### ビルド
```bash
dotnet build
```

### テストスイートの実行
```bash
# 基本
dotnet run --project src/WinFormsE2E -- tests/testapp/01_navigation.json

# 詳細コンソール出力あり
dotnet run --project src/WinFormsE2E -- tests/testapp/01_navigation.json --verbose

# 結果を JSON で保存
dotnet run --project src/WinFormsE2E -- tests/testapp/01_navigation.json --output results.json

# エビデンス有効化（スクリーンショット + HTML レポート、出力先は自動命名）
dotnet run --project src/WinFormsE2E -- tests/testapp/01_navigation.json --evidence

# エビデンス出力先を明示指定
dotnet run --project src/WinFormsE2E -- tests/testapp/01_navigation.json --evidence-output evidence/run1
```

テストランナーの終了コード: `0`（全件合格）、`1`（失敗あり）、`2`（設定・起動エラー）。

---

## テストスイート JSON フォーマット

```json
{
  "suite": "スイート表示名",
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
      "name": "シナリオ名",
      "steps": [
        {
          "action": "click",
          "target": { "automationId": "BtnAdd" },
          "description": "人間が読める説明"
        }
      ]
    }
  ]
}
```

### ターゲットセレクター
要素は以下のフィールドで指定します。
| フィールド | 説明 |
|---|---|
| `name` | `AutomationElement.Current.Name` に一致（コントロールテキストまたは明示的な Name） |
| `automationId` | `AutomationElement.Current.AutomationId` に一致 — WinForms の `Name` プロパティで設定 |

### サポートアクション一覧
| アクション | 説明 |
|---|---|
| `click` | UI 要素をクリック |
| `type` | テキストボックスにテキストを入力（`value` フィールド） |
| `clear` | テキストボックスをクリア |
| `select` | リストまたはコンボボックスの項目を名前で選択 |
| `assert` | 要素の値／テキストを `expect` と照合してアサート |
| `assertWindow` | 現在のウィンドウのプロパティを `expect` と照合してアサート |
| `waitForWindow` | `windowTitle` に一致するウィンドウが表示されるまで待機 |
| `switchWindow` | `windowTitle` に一致するウィンドウにコンテキストを切り替え |
| `closeWindow` | 現在のウィンドウを閉じる |
| `wait` | `ms` ミリ秒だけ実行を一時停止 |
| `inspect` | UI Automation ツリーをコンソールにダンプ（デバッグ用。`ms` を最大深度として使用） |

### アサート `expect` オブジェクト
```json
{
  "property": "Name",
  "operator": "contains",
  "value": "期待するテキスト"
}
```
operator: `equals`, `contains`, `startsWith`, `endsWith`, `notEquals`

---

## 主要アーキテクチャ規約

### TestApp: AutomationId のためのコントロール命名
`TestApp` のすべての操作対象コントロールは、WinForms の `Name` プロパティに PascalCase の識別子を**必ず設定**してください。この `Name` がテスト JSON で使用する `AutomationId` になります。

```csharp
// 良い例 — テスト対象の Name が設定されている
_btnAdd = new Button { Name = "BtnAdd", Text = "追加(&A)", ... };

// 悪い例 — Name 未設定。Text でしかターゲット指定できない
_btnAdd = new Button { Text = "追加(&A)", ... };
```

### StepExecutor: 新しいアクションの追加方法
アクションは `StepExecutor.Execute()` 内の `switch` 式でディスパッチされます。新しいアクションを追加する手順:
1. `Execute()` の `switch` にケースを追加:
   ```csharp
   "expandcollapse" => ExecuteExpandCollapse(step, context),
   ```
2. プライベートメソッド `ExecuteExpandCollapse(TestStep step, TestContext context)` を実装する。
3. 成功時は `StepResult.Pass(...)` を返す。失敗時は例外をスローする（外側の try/catch が `StepResult.Err(...)` を返す）。

### エビデンス収集: IEvidenceCollector
`IEvidenceCollector` は `TestRunner` と `StepExecutor` にフックされます。デフォルトは `null`（任意）。実装クラスは例外を graceful に処理してください — 本番コードからのすべての呼び出しは `try/catch` または `SafeCall()` でラップされています。

現在の実装は `ScreenshotCollector`。`IDbEvidenceProvider` は将来の DB 結果取得向けインターフェーススタブです（Issue #8）。

### シナリオ分離
各シナリオ終了後、`TestRunner.ResetToMainWindow()` がメイン以外のウィンドウを自動的に閉じてメインウィンドウにフォーカスを戻します。これによりシナリオ間の状態汚染を防ぎます。

## プラットフォーム注意事項

- このプロジェクトは **Windows 専用**です。UI Automation (`System.Windows.Automation`) および WinForms は Windows 専用 API です。
- テストランナーは対象アプリを子プロセスとして起動し、COM ベースの UI Automation で制御します。
- `ControlInteractor.Click()` は `InvokePattern.Invoke()` の代わりに `SetForegroundWindow` + `SendKeys.SendWait(" ")` を使用します。これはネイティブ HWND を持つコントロールでモーダルダイアログのデッドロックを避けるためです。
- `ScreenshotCapture` のスクリーンショットは `PrintWindow` ではなく Win32 の `GetWindowRect` + `Graphics.CopyFromScreen`（GDI）を使用します。そのためウィンドウが表示されており最小化されていない必要があります。
