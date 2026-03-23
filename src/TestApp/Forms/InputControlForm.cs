namespace TestApp.Forms;

public class InputControlForm : Form
{
    public InputControlForm()
    {
        Text = "入力制御";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterScreen;

        var lblTitle = new Label
        {
            Text = "入力制御テスト",
            Font = new Font(Font.FontFamily, 14, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 20)
        };

        int y = 70;

        // 1. Alphanumeric only
        var lblAlpha = new Label { Text = "半角英数のみ:", Location = new Point(20, y), AutoSize = true };
        var txtAlpha = new TextBox { Location = new Point(180, y - 3), Size = new Size(250, 25), ImeMode = ImeMode.Disable };
        txtAlpha.KeyPress += (s, e) =>
        {
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
                e.Handled = true;
        };
        var lblAlphaHint = new Label { Text = "※ 英数字とバックスペースのみ入力可", Location = new Point(440, y), AutoSize = true, ForeColor = Color.Gray };
        y += 45;

        // 2. Full-width only (IME on)
        var lblZenkaku = new Label { Text = "全角のみ (IME):", Location = new Point(20, y), AutoSize = true };
        var txtZenkaku = new TextBox { Location = new Point(180, y - 3), Size = new Size(250, 25), ImeMode = ImeMode.On };
        var lblZenkakuHint = new Label { Text = "※ IMEがオンになります", Location = new Point(440, y), AutoSize = true, ForeColor = Color.Gray };
        y += 45;

        // 3. Numeric only
        var lblNumeric = new Label { Text = "数値のみ:", Location = new Point(20, y), AutoSize = true };
        var txtNumeric = new TextBox { Location = new Point(180, y - 3), Size = new Size(250, 25), ImeMode = ImeMode.Disable };
        txtNumeric.KeyPress += (s, e) =>
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
                e.Handled = true;
        };
        var lblNumericHint = new Label { Text = "※ 0-9のみ入力可", Location = new Point(440, y), AutoSize = true, ForeColor = Color.Gray };
        y += 45;

        // 4. Max length
        var lblMaxLen = new Label { Text = "桁数制限 (5桁):", Location = new Point(20, y), AutoSize = true };
        var txtMaxLen = new TextBox { Location = new Point(180, y - 3), Size = new Size(250, 25), MaxLength = 5 };
        var lblMaxLenHint = new Label { Text = "※ 最大5文字まで", Location = new Point(440, y), AutoSize = true, ForeColor = Color.Gray };
        y += 45;

        // 5. Date picker
        var lblDate = new Label { Text = "日付入力:", Location = new Point(20, y), AutoSize = true };
        var dtpDate = new DateTimePicker { Location = new Point(180, y - 3), Size = new Size(250, 25), Format = DateTimePickerFormat.Short };
        var lblDateHint = new Label { Text = "※ DateTimePicker", Location = new Point(440, y), AutoSize = true, ForeColor = Color.Gray };

        var btnBack = new Button { Text = "メインへ戻る(&B)", Location = new Point(20, 520), Size = new Size(120, 30) };
        btnBack.Click += (s, e) => Close();

        Controls.AddRange([
            lblTitle,
            lblAlpha, txtAlpha, lblAlphaHint,
            lblZenkaku, txtZenkaku, lblZenkakuHint,
            lblNumeric, txtNumeric, lblNumericHint,
            lblMaxLen, txtMaxLen, lblMaxLenHint,
            lblDate, dtpDate, lblDateHint,
            btnBack
        ]);
    }
}
