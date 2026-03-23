namespace TestApp.Forms;

public class MainForm : Form
{
    private readonly MenuStrip _menuStrip;
    private readonly StatusStrip _statusStrip;
    private readonly ToolStripStatusLabel _statusLabel;

    public MainForm()
    {
        Text = "テストアプリケーション";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterScreen;

        _menuStrip = new MenuStrip();
        var menuScreens = new ToolStripMenuItem("画面(&S)");
        menuScreens.DropDownItems.Add(new ToolStripMenuItem("データCRUD(&C)", null, (s, e) => OpenForm<CrudForm>()));
        menuScreens.DropDownItems.Add(new ToolStripMenuItem("メッセージ(&M)", null, (s, e) => OpenForm<MessageForm>()));
        menuScreens.DropDownItems.Add(new ToolStripMenuItem("ファンクションキー(&F)", null, (s, e) => OpenForm<FunctionKeyForm>()));
        menuScreens.DropDownItems.Add(new ToolStripMenuItem("入力制御(&I)", null, (s, e) => OpenForm<InputControlForm>()));
        _menuStrip.Items.Add(menuScreens);

        _statusStrip = new StatusStrip();
        _statusLabel = new ToolStripStatusLabel("メイン画面");
        _statusStrip.Items.Add(_statusLabel);

        MainMenuStrip = _menuStrip;
        Controls.Add(_menuStrip);
        Controls.Add(_statusStrip);
    }

    private void OpenForm<T>() where T : Form, new()
    {
        var form = new T();
        form.FormClosed += (s, e) =>
        {
            _statusLabel.Text = "メイン画面";
            Show();
        };
        _statusLabel.Text = form.Text;
        Hide();
        form.Show();
    }
}
