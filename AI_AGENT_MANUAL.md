# AI Agent マニュアル — WinForms E2E テスト JSON 生成ガイド

このドキュメントは、AIエージェントが Windows Forms アプリケーションのソースコードを読み取り、E2Eテスト用の JSON ファイルを高品質に生成するための包括的なリファレンスです。

## 利用フロー

```
INPUT:  対象 WinForms アプリのソースコード + このマニュアル
OUTPUT: テスト用 JSON ファイル
```

**手順:**
1. 対象アプリのソースコードを読み、画面構成・コントロール・動作を把握する
2. テストシナリオを設計する
3. このマニュアルのアクションリファレンスとパターン集を参照して JSON を生成する
4. 検証チェックリスト（セクション9）で品質を確認する

---

## 1. テスト JSON 全体構造

### 1.1 最小テンプレート

```json
{
  "suite": "スイート名",
  "application": {
    "path": "対象アプリの相対パス（バックスラッシュ区切り）",
    "startArgs": "",
    "windowTitle": "メインウィンドウのタイトル",
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
          "action": "アクション名",
          "description": "ステップの説明"
        }
      ]
    }
  ]
}
```

### 1.2 DB 付き完全テンプレート

```json
{
  "suite": "スイート名",
  "application": {
    "path": "src\\App\\bin\\Debug\\net9.0-windows\\App.exe",
    "startArgs": "",
    "windowTitle": "メインウィンドウ",
    "startupWaitMs": 3000
  },
  "database": {
    "connections": {
      "main": {
        "provider": "sqlserver",
        "connectionString": "Data Source=(localdb)\\MSSQLLocalDB;Database=TestDb;Integrated Security=true;TrustServerCertificate=true;"
      }
    }
  },
  "settings": {
    "defaultTimeoutMs": 5000,
    "retryIntervalMs": 200
  },
  "scenarios": []
}
```

### 1.3 各セクション説明

| セクション | 必須 | 説明 |
|---|---|---|
| `suite` | Yes | テストスイートの表示名 |
| `application.path` | Yes | 対象アプリの実行ファイルパス（バックスラッシュ `\\` 区切り） |
| `application.startArgs` | No | 起動時引数 |
| `application.windowTitle` | Yes | メインウィンドウの `Form.Text` と完全一致する文字列 |
| `application.startupWaitMs` | No | 起動待ち時間（推奨: 3000） |
| `database.connections` | No | DB接続定義のマップ。キーが接続名、値が `{provider, connectionString}` |
| `database.connections.*.provider` | — | `"sqlserver"` または `"mssql"` |
| `settings.defaultTimeoutMs` | No | コントロール検索のデフォルトタイムアウト（推奨: 5000） |
| `settings.retryIntervalMs` | No | リトライ間隔（推奨: 200） |

---

## 2. ターゲットセレクター（target）

`target` はテスト対象のUI要素を特定するオブジェクトです。

### 2.1 セレクターフィールド

| フィールド | 型 | 説明 |
|---|---|---|
| `automationId` | string | WinForms の `Control.Name` に対応する AutomationId |
| `name` | string | `AutomationElement.Current.Name`（コントロールの表示テキスト） |
| `className` | string | WinForms のクラス名 |
| `controlType` | string | UI Automation の ControlType |
| `treePath` | int[] | ルートからの子インデックスパス（例: `[0, 2, 1]`） |
| `index` | int | 複数一致時の 0-based インデックス（デフォルト: 0） |

### 2.2 セレクター選択ガイドライン

**優先順位:**

1. **`automationId`（最優先）** — ソースコードの `Control.Name` プロパティと一致。最も安定的
2. **`name`** — メニュー項目（ToolStripMenuItem）やダイアログボタンの選択に使用
3. **`className` + `controlType`** — 組み合わせ条件（AND結合）として補助的に使用
4. **`treePath`** — 他のセレクターで特定できない場合の最終手段

**使い分け例:**
```json
// ボタン・テキストボックス等 → automationId
"target": { "automationId": "BtnAdd" }

// メニュー項目 → name（アクセラレータ付きテキスト）
"target": { "name": "画面(S)" }

// ダイアログのボタン → name
"target": { "name": "はい(Y)" }
"target": { "name": "OK" }

// 複数条件の組み合わせ
"target": { "controlType": "edit", "index": 2 }
```

### 2.3 controlType の有効な値

```
button, checkbox, combobox, edit, hyperlink, image,
list, listitem, menu, menuitem, progressbar, radiobutton,
scrollbar, slider, spinner, statusbar, tab, tabitem, text,
toolbar, tooltip, tree, treeitem, window, datagrid, dataitem,
document, group, header, headeritem, pane, table, titlebar
```

> 注: スペース区切りの別名も使用可能（例: `"check box"`, `"combo box"`, `"list item"`）

---

## 3. アクション完全リファレンス

アクション名は **大文字・小文字を区別しません**（内部で `ToLowerInvariant()` 変換されます）。

### 3.1 UI 操作アクション

#### click

要素をクリックします。

| フィールド | 必須 | 説明 |
|---|---|---|
| `target` | Yes | クリック対象の要素 |

```json
{
  "action": "click",
  "target": { "automationId": "BtnAdd" },
  "description": "追加ボタンをクリック"
}
```

#### doubleclick

要素をダブルクリックします。

| フィールド | 必須 | 説明 |
|---|---|---|
| `target` | Yes | ダブルクリック対象の要素 |

```json
{
  "action": "doubleclick",
  "target": { "automationId": "CrudDataGrid" },
  "description": "DataGridViewの行をダブルクリック"
}
```

#### type

テキストボックスにテキストを入力します。既存値は上書きされます。

| フィールド | 必須 | 説明 |
|---|---|---|
| `target` | Yes | 入力先の要素 |
| `text` | Yes | 入力するテキスト |

```json
{
  "action": "type",
  "target": { "automationId": "TxtItemName" },
  "text": "テストアイテム1",
  "description": "名前を入力"
}
```

> **注意:** テキストフィールドをクリアしたい場合は `"text": ""` を指定してください。

#### sendkeys

キーストロークを送信します。ファンクションキーや修飾キーの入力に使用します。

| フィールド | 必須 | 説明 |
|---|---|---|
| `keys` | Yes | SendKeys 構文のキー文字列 |
| `target` | No | 送信先の要素（省略時はアクティブウィンドウ） |

```json
{
  "action": "sendkeys",
  "keys": "{F5}",
  "description": "F5キーを押下"
}
```

**SendKeys 構文:**
| 記法 | 意味 |
|---|---|
| `{F1}` 〜 `{F12}` | ファンクションキー |
| `{ESC}` | Escape |
| `{ENTER}` | Enter |
| `{TAB}` | Tab |
| `{DELETE}` | Delete |
| `{BACKSPACE}` | Backspace |
| `^c` | Ctrl+C |
| `%{F4}` | Alt+F4 |
| `+{TAB}` | Shift+Tab |

#### select

リストやコンボボックス内の項目を名前で選択します。

| フィールド | 必須 | 説明 |
|---|---|---|
| `target` | Yes | 親コントロール（ComboBox, ListBox等） |
| `value` | Yes | 選択する項目の名前 |

```json
{
  "action": "select",
  "target": { "automationId": "CmbCategory" },
  "value": "飲料",
  "description": "「飲料」を選択"
}
```

> **重要:** ComboBox で `select` を使う前に、必ず `expandcollapse` で展開してください。

#### toggle

チェックボックスやラジオボタンのトグル操作を行います。

| フィールド | 必須 | 説明 |
|---|---|---|
| `target` | Yes | トグル対象の要素 |

```json
{
  "action": "toggle",
  "target": { "automationId": "ChkRememberMe" },
  "description": "「記憶する」チェックボックスを切り替え"
}
```

#### expandcollapse

コンボボックスやツリーノード等の展開・折りたたみ操作を行います。

| フィールド | 必須 | 説明 |
|---|---|---|
| `target` | Yes | 対象の要素 |
| `value` | No | `"expand"` または `"collapse"`（デフォルト: `"expand"`） |

```json
{
  "action": "expandcollapse",
  "target": { "automationId": "CmbCategory" },
  "value": "expand",
  "description": "カテゴリコンボボックスを展開"
}
```

---

### 3.2 アサーションアクション

#### assert

要素のプロパティ値を検証します。

| フィールド | 必須 | 説明 |
|---|---|---|
| `target` | Yes | 検証対象の要素 |
| `expect` | Yes | 期待値オブジェクト `{property, operator, value}` |

```json
{
  "action": "assert",
  "target": { "automationId": "LblMessageResult" },
  "expect": {
    "property": "Name",
    "operator": "contains",
    "value": "成功"
  },
  "description": "結果ラベルに「成功」が含まれることを確認"
}
```

#### assertwindow

現在のウィンドウのタイトルを検証します。

| フィールド | 必須 | 説明 |
|---|---|---|
| `expect` | Yes | 期待値オブジェクト |

```json
{
  "action": "assertwindow",
  "expect": {
    "property": "Name",
    "operator": "contains",
    "value": "データCRUD"
  },
  "description": "CRUD画面のタイトルを確認"
}
```

#### assertdb

SQL クエリを実行し、結果を期待値と照合します。

| フィールド | 必須 | 説明 |
|---|---|---|
| `query` | Yes | `{connectionName, sql}` オブジェクト |
| `expectedRows` | Yes | 期待する結果行の配列。各行はカラム値の配列 |

```json
{
  "action": "assertdb",
  "description": "データがDBに登録されていることを確認",
  "query": {
    "connectionName": "main",
    "sql": "SELECT Id, Name FROM Items ORDER BY Id"
  },
  "expectedRows": [
    [1, "テストアイテム1"],
    [2, "テストアイテム2"]
  ]
}
```

> `expectedRows` が `[]`（空配列）の場合、クエリ結果が0行であることを検証します。
> 値の型は文字列、数値、`null` に対応し、型変換を含む柔軟な比較が行われます。

---

### 3.3 ウィンドウ操作アクション

#### waitforwindow

指定タイトルのウィンドウが表示されるまで待機します。成功時、CurrentWindow が切り替わります。

| フィールド | 必須 | 説明 |
|---|---|---|
| `windowTitle` | Yes | ウィンドウタイトル（ワイルドカード `*`, `?` 対応） |
| `timeoutMs` | No | タイムアウト（デフォルト: `settings.defaultTimeoutMs`） |

```json
{
  "action": "waitforwindow",
  "windowTitle": "データCRUD",
  "description": "CRUD画面が表示されるまで待機"
}
```

#### waitforcontrol

コントロールが表示・利用可能になるまで待機します。

| フィールド | 必須 | 説明 |
|---|---|---|
| `target` | Yes | 待機対象の要素 |
| `timeoutMs` | No | タイムアウト |

```json
{
  "action": "waitforcontrol",
  "target": { "automationId": "TxtAlphanumeric" },
  "description": "入力フィールドが存在することを確認"
}
```

#### switchwindow

指定タイトルのウィンドウにフォーカスを切り替えます。

| フィールド | 必須 | 説明 |
|---|---|---|
| `windowTitle` | Yes | 切り替え先のウィンドウタイトル |
| `timeoutMs` | No | タイムアウト |

```json
{
  "action": "switchwindow",
  "windowTitle": "メッセージ",
  "description": "メッセージ画面に戻る"
}
```

#### closewindow

ウィンドウを閉じます。

| フィールド | 必須 | 説明 |
|---|---|---|
| `windowTitle` | No | 閉じるウィンドウのタイトル（省略時は現在のウィンドウ） |
| `timeoutMs` | No | タイムアウト |

```json
{
  "action": "closewindow",
  "description": "現在のウィンドウを閉じる"
}
```

---

### 3.4 ユーティリティアクション

#### wait

指定ミリ秒だけ実行を一時停止します。

| フィールド | 必須 | 説明 |
|---|---|---|
| `ms` | No | 待機ミリ秒（デフォルト: 1000） |

```json
{
  "action": "wait",
  "ms": 500,
  "description": "DB反映を待機"
}
```

#### executedb

任意の SQL（DDL/DML）を実行します。テーブル作成、データ初期化、クリーンアップ等に使用します。

| フィールド | 必須 | 説明 |
|---|---|---|
| `query` | Yes | `{connectionName, sql}` オブジェクト |

```json
{
  "action": "executedb",
  "description": "テーブルデータを全削除",
  "query": {
    "connectionName": "main",
    "sql": "DELETE FROM Items"
  }
}
```

#### inspect

UI Automation ツリーをコンソールに出力します（デバッグ用）。

| フィールド | 必須 | 説明 |
|---|---|---|
| `ms` | No | 最大探索深度（デフォルト: 5） |

```json
{
  "action": "inspect",
  "ms": 3,
  "description": "UIツリーを3階層まで出力"
}
```

---

## 4. アサート詳細

### 4.1 expect オブジェクト

```json
{
  "property": "プロパティ名",
  "operator": "演算子",
  "value": "期待値"
}
```

### 4.2 対応プロパティ一覧

| プロパティ名 | 説明 | 取得方法 | 値の例 |
|---|---|---|---|
| `Name` | 要素の名前（表示テキスト） | `AutomationElement.Current.Name` | `"追加ボタン"` |
| `AutomationId` | コントロール識別子 | `AutomationElement.Current.AutomationId` | `"BtnAdd"` |
| `ClassName` | WinForms クラス名 | `AutomationElement.Current.ClassName` | `"WindowsForms10.BUTTON"` |
| `IsEnabled` | 有効/無効状態 | `AutomationElement.Current.IsEnabled` | `"True"` / `"False"` |
| `IsOffscreen` | 画面外かどうか | `AutomationElement.Current.IsOffscreen` | `"True"` / `"False"` |
| `Value` | コントロールの値 | `ValuePattern.Current.Value` | `"入力テキスト"` |
| `ToggleState` | トグル状態 | `TogglePattern.Current.ToggleState` | `"On"` / `"Off"` / `"Indeterminate"` |
| `IsSelected` | 選択状態 | `SelectionItemPattern.Current.IsSelected` | `"True"` / `"False"` |
| `BoundingRectangle` | 要素の位置とサイズ | `AutomationElement.Current.BoundingRectangle` | 矩形文字列 |
| `ItemStatus` | アイテムのステータス | `AutomationElement.Current.ItemStatus` | 文字列 |

> **DataGridView の `Value`:** DataGrid/Table/List コントロールの場合、全セルの値が `" | "` 区切りで結合された文字列として返されます。`contains` 演算子で特定セル値の存在を確認するのが一般的です。

### 4.3 対応演算子一覧

| 演算子 | 説明 | 大文字小文字 |
|---|---|---|
| `equals` | 完全一致 | 区別する |
| `notequals` | 不一致 | 区別する |
| `contains` | 部分一致 | 区別する |
| `containsignorecase` | 部分一致 | 区別しない |
| `startswith` | 前方一致 | 区別する |
| `endswith` | 後方一致 | 区別する |
| `matches` | 正規表現一致 | パターン依存 |
| `equalsignorecase` | 完全一致 | 区別しない |

---

## 5. ソースコードの読み方ガイド

テスト JSON を生成するために、対象アプリのソースコードから以下の情報を抽出する必要があります。

### 5.1 AutomationId の特定

WinForms の `Control.Name` プロパティが UI Automation の `AutomationId` になります。

```csharp
// ソースコード
_btnAdd = new Button { Name = "BtnAdd", Text = "追加(&A)" };

// → テスト JSON
"target": { "automationId": "BtnAdd" }
```

**命名規則（一般的なプレフィックス）:**
| プレフィックス | コントロール種別 |
|---|---|
| `Btn` | Button |
| `Txt` | TextBox |
| `Lbl` | Label |
| `Cmb` | ComboBox |
| `Chk` | CheckBox |
| `Rdb` | RadioButton |
| `Dgv` / `DataGrid` | DataGridView |
| `Dtp` | DateTimePicker |
| `Menu` | ToolStripMenuItem |

### 5.2 ウィンドウタイトルの特定

`Form.Text` プロパティがウィンドウタイトルになります。

```csharp
// ソースコード
public class CrudForm : Form
{
    public CrudForm()
    {
        Text = "データCRUD";
    }
}

// → テスト JSON
"windowTitle": "データCRUD"
```

### 5.3 メニュー構造の読み方

`ToolStripMenuItem` のコンストラクタ第1引数がメニューテキストです。
アクセラレータキー `&` は UI Automation では括弧表記に変換されます。

```csharp
// ソースコード
new ToolStripMenuItem("画面(&S)", null, new ToolStripMenuItem[] { ... });
new ToolStripMenuItem("データCRUD(&C)", null, (s, e) => OpenForm<CrudForm>());

// → UI Automation での Name
"画面(S)"
"データCRUD(C)"

// → テスト JSON（メニュー操作）
{ "action": "click", "target": { "name": "画面(S)" } }
{ "action": "click", "target": { "name": "データCRUD(C)" } }
```

> **変換規則:** ソースの `&X` → UI Automation では `(X)`

### 5.4 イベントハンドラからの動作推定

```csharp
// ボタンクリックイベント
_btnDelete.Click += (s, e) =>
{
    var result = MessageBox.Show("削除しますか？", "確認", MessageBoxButtons.YesNo);
    if (result == DialogResult.Yes)
    {
        _lblResult.Text = "削除が実行されました";
    }
};
```

**読み取るべき情報:**
- `MessageBox.Show` の第1引数 → ダイアログの本文
- `MessageBox.Show` の第2引数 → `windowTitle` に使用するダイアログタイトル
- `MessageBoxButtons.YesNo` → 「はい(Y)」「いいえ(N)」ボタンが表示される
- `DialogResult.Yes` の分岐 → 「はい」クリック後の期待される結果
- `_lblResult.Text = "..."` → assert で検証する期待値

### 5.5 コントロール種別からアクションへのマッピング

| コントロール | 主なアクション | assert プロパティ |
|---|---|---|
| Button | `click` | — |
| TextBox | `type` | `Value` |
| Label | — | `Name` |
| ComboBox | `expandcollapse` → `select` → `expandcollapse` | `Value` |
| CheckBox | `toggle` | `ToggleState` |
| RadioButton | `toggle` | `IsSelected` |
| DataGridView | `click`（行選択） | `Value`（`contains` で検証） |
| ToolStripMenuItem | `click` | — |
| DateTimePicker | `type` | `Value` |

---

## 6. テストパターン集

### 6.1 画面遷移パターン

メニューからサブ画面を開き、タイトルを確認するパターン。

```json
{
  "name": "画面遷移テスト",
  "steps": [
    {
      "action": "click",
      "target": { "name": "画面(S)" },
      "description": "画面メニューをクリック"
    },
    {
      "action": "click",
      "target": { "name": "データCRUD(C)" },
      "description": "データCRUDメニューをクリック"
    },
    {
      "action": "waitForWindow",
      "windowTitle": "データCRUD",
      "description": "CRUD画面が表示されるまで待機"
    },
    {
      "action": "assertWindow",
      "expect": {
        "property": "Name",
        "operator": "contains",
        "value": "データCRUD"
      },
      "description": "CRUD画面のタイトルを確認"
    }
  ]
}
```

### 6.2 フォーム入力パターン

テキスト入力 → ボタンクリック → 結果検証のパターン。

```json
{
  "name": "データ追加テスト",
  "steps": [
    {
      "action": "type",
      "target": { "automationId": "TxtItemName" },
      "text": "テストアイテム1",
      "description": "名前を入力"
    },
    {
      "action": "type",
      "target": { "automationId": "TxtItemDescription" },
      "text": "説明文",
      "description": "説明を入力"
    },
    {
      "action": "click",
      "target": { "automationId": "BtnAdd" },
      "description": "追加ボタンをクリック"
    },
    {
      "action": "wait",
      "ms": 500,
      "description": "処理完了を待機"
    },
    {
      "action": "assert",
      "target": { "automationId": "CrudDataGrid" },
      "expect": {
        "property": "Value",
        "operator": "contains",
        "value": "テストアイテム1"
      },
      "description": "DataGridViewに追加データが表示されていることを確認"
    }
  ]
}
```

### 6.3 ダイアログ操作パターン

ボタンクリック → ダイアログ表示 → ダイアログ操作 → 元画面に戻って結果確認のパターン。

```json
{
  "name": "削除確認ダイアログテスト",
  "steps": [
    {
      "action": "click",
      "target": { "automationId": "BtnDeleteConfirm" },
      "description": "削除確認ボタンをクリック"
    },
    {
      "action": "waitForWindow",
      "windowTitle": "削除確認",
      "timeoutMs": 3000,
      "description": "削除確認ダイアログが表示されるまで待機"
    },
    {
      "action": "click",
      "target": { "name": "はい(Y)" },
      "description": "「はい」ボタンをクリック"
    },
    {
      "action": "switchWindow",
      "windowTitle": "メッセージ",
      "description": "メッセージ画面に戻る"
    },
    {
      "action": "assert",
      "target": { "automationId": "LblMessageResult" },
      "expect": {
        "property": "Name",
        "operator": "contains",
        "value": "削除が実行されました"
      },
      "description": "結果ラベルに削除実行メッセージが表示されていることを確認"
    }
  ]
}
```

**ダイアログボタンの name 一覧（標準 MessageBox）:**
| MessageBoxButtons | 表示ボタン |
|---|---|
| OK | `"OK"` |
| YesNo | `"はい(Y)"`, `"いいえ(N)"` |
| YesNoCancel | `"はい(Y)"`, `"いいえ(N)"`, `"キャンセル"` |
| OKCancel | `"OK"`, `"キャンセル"` |

### 6.4 コンボボックス選択パターン

ComboBox の操作は「展開 → 選択 → 閉じる」の3ステップが必要です。

```json
{
  "name": "コンボボックス項目選択",
  "steps": [
    {
      "action": "expandcollapse",
      "target": { "automationId": "CmbCategory" },
      "value": "expand",
      "description": "コンボボックスを展開"
    },
    {
      "action": "select",
      "target": { "automationId": "CmbCategory" },
      "value": "飲料",
      "description": "「飲料」を選択"
    },
    {
      "action": "expandcollapse",
      "target": { "automationId": "CmbCategory" },
      "value": "collapse",
      "description": "コンボボックスを閉じる"
    },
    {
      "action": "wait",
      "ms": 300,
      "description": "選択反映を待機"
    },
    {
      "action": "assert",
      "target": { "automationId": "LblSelectedInfo" },
      "expect": {
        "property": "Name",
        "operator": "contains",
        "value": "飲料"
      },
      "description": "選択結果が表示されていることを確認"
    }
  ]
}
```

### 6.5 ファンクションキーパターン

```json
{
  "name": "F5キーテスト",
  "steps": [
    {
      "action": "sendkeys",
      "keys": "{F5}",
      "description": "F5キーを押下"
    },
    {
      "action": "wait",
      "ms": 300,
      "description": "処理完了を待機"
    },
    {
      "action": "assert",
      "target": { "automationId": "LblFKeyResult" },
      "expect": {
        "property": "Name",
        "operator": "contains",
        "value": "F5 - リフレッシュ"
      },
      "description": "結果ラベルにF5リフレッシュが表示されていることを確認"
    }
  ]
}
```

### 6.6 DB セットアップ/テアダウンパターン

テスト前のデータ初期化とテスト後のクリーンアップ。

```json
{
  "name": "DB初期化",
  "steps": [
    {
      "action": "executedb",
      "description": "テーブルを作成（存在しない場合）",
      "query": {
        "connectionName": "main",
        "sql": "IF OBJECT_ID('Items', 'U') IS NULL CREATE TABLE Items (Id int IDENTITY(1,1) PRIMARY KEY, Name nvarchar(200) NOT NULL, Description nvarchar(500))"
      }
    },
    {
      "action": "executedb",
      "description": "データを全削除",
      "query": {
        "connectionName": "main",
        "sql": "DELETE FROM Items"
      }
    },
    {
      "action": "executedb",
      "description": "IDENTITYをリセット",
      "query": {
        "connectionName": "main",
        "sql": "DBCC CHECKIDENT ('Items', RESEED, 0)"
      }
    },
    {
      "action": "assertdb",
      "description": "テーブルが空であることを確認",
      "query": {
        "connectionName": "main",
        "sql": "SELECT Id, Name, Description FROM Items"
      },
      "expectedRows": []
    }
  ]
}
```

### 6.7 DB + UI 統合テストパターン

UI操作でデータを登録し、DBの内容をアサーションで検証するパターン。

```json
{
  "name": "UI操作によるDB登録確認",
  "steps": [
    {
      "action": "type",
      "target": { "automationId": "TxtDbItemName" },
      "text": "テストアイテム1",
      "description": "名前を入力"
    },
    {
      "action": "type",
      "target": { "automationId": "TxtDbItemDescription" },
      "text": "テスト説明1",
      "description": "説明を入力"
    },
    {
      "action": "click",
      "target": { "automationId": "BtnDbAdd" },
      "description": "追加ボタンをクリック"
    },
    {
      "action": "wait",
      "ms": 500,
      "description": "DB反映を待機"
    },
    {
      "action": "assertdb",
      "description": "データがDBに登録されていることを確認",
      "query": {
        "connectionName": "main",
        "sql": "SELECT Id, Name, Description FROM Items ORDER BY Id"
      },
      "expectedRows": [
        [1, "テストアイテム1", "テスト説明1"]
      ]
    }
  ]
}
```

### 6.8 入力値クリア → 再入力パターン

既存の値をクリアしてから新しい値を入力するパターン。

```json
{
  "name": "データ更新",
  "steps": [
    {
      "action": "type",
      "target": { "automationId": "TxtDbItemName" },
      "text": "",
      "description": "名前フィールドをクリア"
    },
    {
      "action": "type",
      "target": { "automationId": "TxtDbItemName" },
      "text": "更新後の名前",
      "description": "新しい名前を入力"
    }
  ]
}
```

---

## 7. シナリオ設計ガイドライン

### 7.1 シナリオ分離の原則

- 各シナリオ終了後、テストランナーが自動的にメイン以外のウィンドウを閉じ、メインウィンドウにフォーカスを戻します（`ResetToMainWindow`）
- **各シナリオはメインウィンドウから開始される前提**で設計してください
- シナリオ間でUI状態は引き継がれません
- **ただし DB データはシナリオ間で永続化される**ため、テスト間のデータ依存に注意してください

### 7.2 シナリオ構成の推奨パターン

1. **セットアップシナリオ**（DB使用時）: テーブル作成、データ初期化
2. **操作テストシナリオ**: 画面遷移 → 操作 → 検証
3. **クリーンアップシナリオ**（DB使用時）: テーブル削除

### 7.3 ステップ記述のルール

- **全ステップに `description` を記載する**（HTML レポートの可読性向上のため）
- description は**日本語で、人間が読んで理解できる説明**を記載する
- UI操作後にアサーションが必要な場合は `wait` を挟む（300〜500ms 推奨）

---

## 8. 注意事項・よくある間違い

### 8.1 メニューのアクセラレータ変換

ソースの `"画面(&S)"` は UI Automation では `"画面(S)"` として認識されます。`&` は `()` に変換してください。

```
ソース: "画面(&S)"     → JSON name: "画面(S)"
ソース: "データCRUD(&C)" → JSON name: "データCRUD(C)"
```

### 8.2 wait vs waitForWindow vs waitForControl

| 用途 | 使うべきアクション |
|---|---|
| 固定時間の待機（DB反映、UI更新等） | `wait` |
| 新しいウィンドウ/ダイアログの出現待ち | `waitforwindow` |
| コントロールの出現/有効化待ち | `waitforcontrol` |

> ダイアログ待ちに `wait` を使わないでください。`waitforwindow` はタイムアウト付きでリトライするため、より堅牢です。

### 8.3 ComboBox 操作の必須手順

ComboBox で `select` する前に**必ず `expandcollapse expand` を実行**してください。
`select` 後は **`expandcollapse collapse` でドロップダウンを閉じる**ことを推奨します。
閉じないと後続操作でウィンドウ位置がずれる可能性があります。

```
expandcollapse expand → select → expandcollapse collapse → wait
```

### 8.4 ダイアログ操作後の switchWindow

ダイアログ（MessageBox等）を閉じた後は、**`switchwindow` で元の画面に戻る**必要があります。
`waitforwindow` はウィンドウ検出時に自動で CurrentWindow を切り替えますが、ダイアログが閉じた後は元のウィンドウへの明示的な切り替えが必要です。

### 8.5 type アクションの注意点

- `type` は `ValuePattern.SetValue()` を使用し、既存値を**上書き**します
- フィールドをクリアしたい場合は `"text": ""` を指定してください
- **JSON キーは `"text"` であり、`"value"` ではありません**

### 8.6 application.path のパス区切り

JSON 内のパスはバックスラッシュをエスケープして `\\` で区切ります。

```json
"path": "src\\TestApp\\bin\\Debug\\net9.0-windows\\TestApp.exe"
```

### 8.7 DB テストのシナリオ間データ依存

DB データはシナリオ間で永続化されます。前のシナリオで登録したデータは次のシナリオでも存在します。
IDENTITY 列の値もリセットされないため、連番の期待値に注意してください。

---

## 9. 出力 JSON 検証チェックリスト

生成した JSON を提出する前に、以下を確認してください。

- [ ] `suite` 名が設定されている
- [ ] `application.path` が正しい相対パス（`\\` 区切り）
- [ ] `application.windowTitle` が対象アプリの `Form.Text` と完全一致
- [ ] 全シナリオに `name` が設定されている
- [ ] 全ステップに `action` が設定されている
- [ ] 全ステップに `description` が設定されている
- [ ] `target` が必要なアクションに `target` が設定されている
- [ ] `type` アクションに `text` フィールドがある（`value` ではない）
- [ ] `select` アクションに `value` フィールドがある
- [ ] 全 `assert` / `assertwindow` に `expect` が設定されている
- [ ] `expect.operator` が有効な演算子名である
- [ ] DB アクション使用時にトップレベルの `database` セクションが定義されている
- [ ] 画面遷移後に `waitforwindow` が挿入されている
- [ ] ダイアログ操作後に `switchwindow` で元の画面に戻っている
- [ ] ComboBox の `select` 前に `expandcollapse expand` がある
- [ ] ComboBox の `select` 後に `expandcollapse collapse` がある
- [ ] UI操作後のアサーション前に適切な `wait` が挿入されている
- [ ] メニュー名のアクセラレータが `&X` → `(X)` に変換されている
- [ ] JSON が構文的に有効である（末尾カンマ、引用符等）
