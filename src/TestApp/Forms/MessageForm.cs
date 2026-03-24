namespace TestApp.Forms;

public class MessageForm : Form
{
    private readonly Label _lblResult;
    private readonly TextBox _txtInput;

    public MessageForm()
    {
        Text = "メッセージ";
        Name = "MessageForm";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterScreen;

        var lblTitle = new Label { Text = "メッセージ条件分岐テスト", Font = new Font(Font.FontFamily, 14, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };

        // Validation input
        var lblInput = new Label { Text = "入力値:", Location = new Point(20, 70), AutoSize = true };
        _txtInput = new TextBox { Name = "TxtMessageInput", Location = new Point(80, 67), Size = new Size(200, 25) };

        // Buttons
        var btnDeleteConfirm = new Button { Name = "BtnDeleteConfirm", Text = "削除確認(&D)", Location = new Point(20, 110), Size = new Size(140, 35) };
        btnDeleteConfirm.Click += BtnDeleteConfirm_Click;

        var btnSaveConfirm = new Button { Name = "BtnSaveConfirm", Text = "保存確認(&S)", Location = new Point(170, 110), Size = new Size(140, 35) };
        btnSaveConfirm.Click += BtnSaveConfirm_Click;

        var btnValidation = new Button { Name = "BtnValidation", Text = "バリデーション(&V)", Location = new Point(320, 110), Size = new Size(140, 35) };
        btnValidation.Click += BtnValidation_Click;

        var btnSuccess = new Button { Name = "BtnSuccess", Text = "成功通知(&N)", Location = new Point(470, 110), Size = new Size(140, 35) };
        btnSuccess.Click += BtnSuccess_Click;

        // Result display
        _lblResult = new Label
        {
            Name = "LblMessageResult",
            Text = "結果: (未操作)",
            Location = new Point(20, 170),
            AutoSize = true,
            Font = new Font(Font.FontFamily, 11)
        };

        var btnBack = new Button { Name = "BtnMessageBack", Text = "メインへ戻る(&B)", Location = new Point(20, 520), Size = new Size(120, 30) };
        btnBack.Click += (s, e) => Close();

        Controls.AddRange([lblTitle, lblInput, _txtInput, btnDeleteConfirm, btnSaveConfirm, btnValidation, btnSuccess, _lblResult, btnBack]);
    }

    private void BtnDeleteConfirm_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "このデータを削除しますか？",
            "削除確認",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        _lblResult.Text = result == DialogResult.Yes
            ? "結果: 削除が実行されました"
            : "結果: 削除がキャンセルされました";
    }

    private void BtnSaveConfirm_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "変更を保存しますか？",
            "保存確認",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);

        _lblResult.Text = result switch
        {
            DialogResult.Yes => "結果: 保存が実行されました",
            DialogResult.No => "結果: 保存せずに続行します",
            _ => "結果: 操作がキャンセルされました"
        };
    }

    private void BtnValidation_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtInput.Text))
        {
            MessageBox.Show(
                "入力値が空です。値を入力してください。",
                "入力エラー",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            _lblResult.Text = "結果: バリデーションエラー（空入力）";
            return;
        }

        _lblResult.Text = $"結果: バリデーション成功（入力値: {_txtInput.Text}）";
    }

    private void BtnSuccess_Click(object? sender, EventArgs e)
    {
        MessageBox.Show(
            "操作が正常に完了しました。",
            "成功",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        _lblResult.Text = "結果: 成功通知を表示しました";
    }
}
