namespace TestApp.Forms;

public class InputControlForm : Form
{
    public InputControlForm()
    {
        Text = "入力制御";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterScreen;

        var label = new Label
        {
            Text = "入力制御画面",
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
