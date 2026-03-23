namespace TestApp.Forms;

public class MessageForm : Form
{
    public MessageForm()
    {
        Text = "メッセージ";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterScreen;

        var label = new Label
        {
            Text = "メッセージ画面",
            AutoSize = true,
            Location = new Point(20, 40)
        };
        Controls.Add(label);

        var btnBack = new Button
        {
            Text = "メインへ戻る(&B)",
            Location = new Point(20, 520),
            Size = new Size(120, 30)
        };
        btnBack.Click += (s, e) => Close();
        Controls.Add(btnBack);
    }
}
