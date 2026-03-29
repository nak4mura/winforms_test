namespace TestApp.Forms;

public class FileOutputForm : Form
{
    private readonly TextBox _txtContent;
    private readonly Label _lblResult;

    public FileOutputForm()
    {
        Text = "ファイル出力";
        Name = "FileOutputForm";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterScreen;

        var lblTitle = new Label
        {
            Text = "ファイル出力テスト",
            Font = new Font(Font.FontFamily, 14, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 20)
        };

        var lblContent = new Label { Text = "出力内容:", Location = new Point(20, 70), AutoSize = true };
        _txtContent = new TextBox
        {
            Name = "TxtFileContent",
            Location = new Point(100, 67),
            Size = new Size(400, 25)
        };

        var btnSave = new Button
        {
            Name = "BtnSaveFile",
            Text = "ファイル保存(&S)",
            Location = new Point(20, 110),
            Size = new Size(140, 35)
        };
        btnSave.Click += BtnSave_Click;

        _lblResult = new Label
        {
            Name = "LblFileResult",
            Text = "結果: (未操作)",
            Location = new Point(20, 170),
            AutoSize = true,
            Font = new Font(Font.FontFamily, 11)
        };

        var btnBack = new Button
        {
            Name = "BtnFileBack",
            Text = "メインへ戻る(&B)",
            Location = new Point(20, 520),
            Size = new Size(120, 30)
        };
        btnBack.Click += (s, e) => Close();

        Controls.AddRange([lblTitle, lblContent, _txtContent, btnSave, _lblResult, btnBack]);
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "テキストファイル (*.txt)|*.txt|すべてのファイル (*.*)|*.*",
            DefaultExt = "txt",
            Title = "名前を付けて保存"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            File.WriteAllText(dialog.FileName, _txtContent.Text);
            _lblResult.Text = $"結果: ファイルを保存しました ({dialog.FileName})";
        }
        else
        {
            _lblResult.Text = "結果: 保存がキャンセルされました";
        }
    }
}
