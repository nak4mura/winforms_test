namespace TestApp.Forms;

public class AboutDialog : Form
{
    public AboutDialog()
    {
        Text = "バージョン情報";
        Name = "AboutDialog";
        Size = new Size(350, 200);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var lblAppName = new Label
        {
            Name = "LblAppName",
            Text = "テストアプリケーション",
            Font = new Font(Font.FontFamily, 14, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 20)
        };

        var lblVersion = new Label
        {
            Name = "LblVersion",
            Text = "バージョン 1.0.0",
            AutoSize = true,
            Location = new Point(20, 60)
        };

        var btnOk = new Button
        {
            Name = "BtnAboutOk",
            Text = "OK",
            DialogResult = DialogResult.OK,
            Location = new Point(125, 120),
            Size = new Size(100, 30)
        };

        AcceptButton = btnOk;
        Controls.AddRange([lblAppName, lblVersion, btnOk]);
    }
}
