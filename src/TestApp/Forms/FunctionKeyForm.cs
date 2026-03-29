namespace TestApp.Forms;

public class FunctionKeyForm : Form
{
    private readonly Label _lblResult;
    private readonly Label _lblLog;
    private readonly StatusStrip _statusStrip;
    private int _refreshCount;

    public FunctionKeyForm()
    {
        Text = "ファンクションキー";
        Name = "FunctionKeyForm";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;

        var lblTitle = new Label
        {
            Text = "ファンクションキーテスト",
            Font = new Font(Font.FontFamily, 14, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 20)
        };

        var lblGuide = new Label
        {
            Name = "LblKeyGuide",
            Text = "F1: ヘルプ  |  F5: リフレッシュ  |  F10: 保存  |  Esc: 閉じる",
            AutoSize = true,
            Location = new Point(20, 60),
            Font = new Font(Font.FontFamily, 10)
        };

        _lblResult = new Label
        {
            Name = "LblFKeyResult",
            Text = "最後の操作: (未操作)",
            AutoSize = true,
            Location = new Point(20, 100),
            Font = new Font(Font.FontFamily, 11)
        };

        _lblLog = new Label
        {
            Name = "LblFKeyLog",
            Text = "",
            Location = new Point(20, 140),
            Size = new Size(740, 340),
            BorderStyle = BorderStyle.FixedSingle
        };

        // StatusStrip with function key guide
        _statusStrip = new StatusStrip { Name = "FKeyStatusStrip" };
        _statusStrip.Items.Add(new ToolStripStatusLabel("F1:ヘルプ"));
        _statusStrip.Items.Add(new ToolStripSeparator());
        _statusStrip.Items.Add(new ToolStripStatusLabel("F5:更新"));
        _statusStrip.Items.Add(new ToolStripSeparator());
        _statusStrip.Items.Add(new ToolStripStatusLabel("F10:保存"));
        _statusStrip.Items.Add(new ToolStripSeparator());
        _statusStrip.Items.Add(new ToolStripStatusLabel("Esc:閉じる"));

        var btnBack = new Button { Name = "BtnFKeyBack", Text = "メインへ戻る(&B)", Location = new Point(20, 490), Size = new Size(120, 30) };
        btnBack.Click += (s, e) => Close();

        Controls.AddRange([lblTitle, lblGuide, _lblResult, _lblLog, _statusStrip, btnBack]);
        KeyDown += FunctionKeyForm_KeyDown;
    }

    private void FunctionKeyForm_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.F1:
                e.Handled = true;
                _lblResult.Text = "最後の操作: F1 - ヘルプ";
                AppendLog("F1キー: ヘルプを表示しました");
                MessageBox.Show(
                    "ファンクションキーテスト画面のヘルプです。\n\nF1: このヘルプを表示\nF5: データをリフレッシュ\nF10: データを保存\nEsc: 画面を閉じる",
                    "ヘルプ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                break;

            case Keys.F5:
                e.Handled = true;
                _refreshCount++;
                _lblResult.Text = $"最後の操作: F5 - リフレッシュ (回数: {_refreshCount})";
                AppendLog($"F5キー: データをリフレッシュしました（{_refreshCount}回目）");
                break;

            case Keys.F10:
                e.Handled = true;
                _lblResult.Text = "最後の操作: F10 - 保存";
                AppendLog("F10キー: データを保存しました");
                MessageBox.Show("データを保存しました。", "保存完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
                break;

            case Keys.Escape:
                e.Handled = true;
                AppendLog("Escキー: 画面を閉じます");
                Close();
                break;
        }
    }

    private void AppendLog(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        _lblLog.Text = $"[{timestamp}] {message}\n{_lblLog.Text}";
    }
}
