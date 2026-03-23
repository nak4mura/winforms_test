namespace TestApp.Forms;

public class CrudForm : Form
{
    public CrudForm()
    {
        Text = "データCRUD";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterScreen;

        var label = new Label
        {
            Text = "データCRUD画面",
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
