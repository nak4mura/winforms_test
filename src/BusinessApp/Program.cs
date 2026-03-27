using BusinessApp.Data;
using BusinessApp.Forms;

namespace BusinessApp;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            DatabaseInitializer.Initialize(AppSettings.ConnectionString);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"データベースの初期化に失敗しました。\nLocalDBが利用可能か確認してください。\n\n{ex.Message}",
                "起動エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Application.Run(new MainForm());
    }
}
