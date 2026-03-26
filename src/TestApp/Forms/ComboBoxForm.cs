namespace TestApp.Forms;

public class ComboBoxForm : Form
{
    private readonly ComboBox _cmbCategory;
    private readonly ComboBox _cmbSubCategory;
    private readonly Label _lblSelectedInfo;

    private static readonly Dictionary<string, string[]> SubCategories = new()
    {
        { "食品", new[] { "肉類", "魚介類", "野菜", "果物" } },
        { "飲料", new[] { "お茶", "コーヒー", "ジュース", "水" } },
        { "日用品", new[] { "洗剤", "トイレ用品", "掃除用品", "文房具" } }
    };

    public ComboBoxForm()
    {
        Text = "コンボボックス";
        Name = "ComboBoxForm";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterScreen;

        var lblTitle = new Label
        {
            Text = "コンボボックステスト",
            Font = new Font(Font.FontFamily, 14, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 20)
        };

        int y = 70;

        // Category ComboBox
        var lblCategory = new Label { Text = "カテゴリ:", Location = new Point(20, y), AutoSize = true };
        _cmbCategory = new ComboBox
        {
            Name = "CmbCategory",
            Location = new Point(180, y - 3),
            Size = new Size(250, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cmbCategory.Items.AddRange(new object[] { "食品", "飲料", "日用品" });
        _cmbCategory.SelectedIndexChanged += OnCategoryChanged;
        y += 45;

        // SubCategory ComboBox
        var lblSubCategory = new Label { Text = "サブカテゴリ:", Location = new Point(20, y), AutoSize = true };
        _cmbSubCategory = new ComboBox
        {
            Name = "CmbSubCategory",
            Location = new Point(180, y - 3),
            Size = new Size(250, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cmbSubCategory.SelectedIndexChanged += OnSubCategoryChanged;
        y += 45;

        // Selected info label
        var lblInfoCaption = new Label { Text = "選択結果:", Location = new Point(20, y), AutoSize = true };
        _lblSelectedInfo = new Label
        {
            Name = "LblSelectedInfo",
            Text = "(未選択)",
            Location = new Point(180, y),
            AutoSize = true
        };
        y += 45;

        var btnBack = new Button
        {
            Name = "BtnComboBack",
            Text = "メインへ戻る(&B)",
            Location = new Point(20, 520),
            Size = new Size(120, 30)
        };
        btnBack.Click += (s, e) => Close();

        Controls.AddRange([
            lblTitle,
            lblCategory, _cmbCategory,
            lblSubCategory, _cmbSubCategory,
            lblInfoCaption, _lblSelectedInfo,
            btnBack
        ]);
    }

    private void OnCategoryChanged(object? sender, EventArgs e)
    {
        _cmbSubCategory.Items.Clear();
        _cmbSubCategory.Text = "";

        if (_cmbCategory.SelectedItem is string category && SubCategories.TryGetValue(category, out var subs))
        {
            _cmbSubCategory.Items.AddRange(subs);
        }

        UpdateSelectedInfo();
    }

    private void OnSubCategoryChanged(object? sender, EventArgs e)
    {
        UpdateSelectedInfo();
    }

    private void UpdateSelectedInfo()
    {
        var category = _cmbCategory.SelectedItem as string ?? "";
        var subCategory = _cmbSubCategory.SelectedItem as string ?? "";

        if (string.IsNullOrEmpty(category))
        {
            _lblSelectedInfo.Text = "(未選択)";
        }
        else if (string.IsNullOrEmpty(subCategory))
        {
            _lblSelectedInfo.Text = $"カテゴリ: {category}";
        }
        else
        {
            _lblSelectedInfo.Text = $"カテゴリ: {category} / サブカテゴリ: {subCategory}";
        }
    }
}
