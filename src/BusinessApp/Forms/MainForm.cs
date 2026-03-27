using BusinessApp.Data;
using BusinessApp.Models;

namespace BusinessApp.Forms;

public class MainForm : Form
{
    private readonly EmployeeRepository _employeeRepo;
    private readonly DepartmentRepository _deptRepo;

    private MenuStrip _menuStrip = null!;
    private TextBox _txtSearch = null!;
    private ComboBox _cmbDepartment = null!;
    private ComboBox _cmbStatus = null!;
    private Button _btnSearch = null!;
    private Button _btnClear = null!;
    private DataGridView _dgvEmployees = null!;
    private Button _btnAdd = null!;
    private Button _btnEdit = null!;
    private Button _btnDelete = null!;
    private StatusStrip _statusStrip = null!;
    private ToolStripStatusLabel _lblStatus = null!;
    private ToolStripStatusLabel _lblCount = null!;

    public MainForm()
    {
        _employeeRepo = new EmployeeRepository(AppSettings.ConnectionString);
        _deptRepo = new DepartmentRepository(AppSettings.ConnectionString);
        InitializeComponent();
        LoadDepartmentFilter();
        SearchEmployees();
    }

    private void InitializeComponent()
    {
        Text = "従業員管理システム";
        Size = new Size(1100, 700);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 500);

        // Menu
        _menuStrip = new MenuStrip();
        var fileMenu = new ToolStripMenuItem("ファイル(&F)");
        fileMenu.DropDownItems.Add("終了(&X)", null, (_, _) => Close());
        var masterMenu = new ToolStripMenuItem("マスタ(&M)");
        masterMenu.DropDownItems.Add("部署管理(&D)", null, (_, _) => OpenDepartmentForm());
        var helpMenu = new ToolStripMenuItem("ヘルプ(&H)");
        helpMenu.DropDownItems.Add("バージョン情報(&A)", null, (_, _) =>
            MessageBox.Show("従業員管理システム v1.0\n業務アプリケーション", "バージョン情報",
                MessageBoxButtons.OK, MessageBoxIcon.Information));
        _menuStrip.Items.AddRange([fileMenu, masterMenu, helpMenu]);

        // Search panel
        var searchPanel = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(8, 4, 8, 4) };

        var lblSearch = new Label { Text = "検索:", Location = new Point(8, 15), AutoSize = true };
        _txtSearch = new TextBox { Location = new Point(50, 12), Width = 200 };
        _txtSearch.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) SearchEmployees(); };

        var lblDept = new Label { Text = "部署:", Location = new Point(270, 15), AutoSize = true };
        _cmbDepartment = new ComboBox
        {
            Location = new Point(310, 12), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList
        };

        var lblStatus = new Label { Text = "状態:", Location = new Point(480, 15), AutoSize = true };
        _cmbStatus = new ComboBox
        {
            Location = new Point(520, 12), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cmbStatus.Items.AddRange(["すべて", "在籍", "退職"]);
        _cmbStatus.SelectedIndex = 0;

        _btnSearch = new Button { Text = "検索", Location = new Point(640, 10), Width = 75 };
        _btnSearch.Click += (_, _) => SearchEmployees();

        _btnClear = new Button { Text = "クリア", Location = new Point(720, 10), Width = 75 };
        _btnClear.Click += (_, _) => ClearSearch();

        searchPanel.Controls.AddRange([lblSearch, _txtSearch, lblDept, _cmbDepartment, lblStatus, _cmbStatus, _btnSearch, _btnClear]);

        // DataGridView
        _dgvEmployees = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = SystemColors.Window,
            RowHeadersVisible = false,
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(245, 245, 250) }
        };
        _dgvEmployees.CellDoubleClick += (_, _) => EditEmployee();

        // Button panel
        var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 50, Padding = new Padding(8) };
        _btnAdd = new Button { Text = "新規追加(&N)", Width = 100, Location = new Point(8, 10) };
        _btnAdd.Click += (_, _) => AddEmployee();
        _btnEdit = new Button { Text = "編集(&E)", Width = 100, Location = new Point(116, 10) };
        _btnEdit.Click += (_, _) => EditEmployee();
        _btnDelete = new Button { Text = "削除(&D)", Width = 100, Location = new Point(224, 10) };
        _btnDelete.Click += (_, _) => DeleteEmployee();
        buttonPanel.Controls.AddRange([_btnAdd, _btnEdit, _btnDelete]);

        // Status bar
        _statusStrip = new StatusStrip();
        _lblStatus = new ToolStripStatusLabel("準備完了") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
        _lblCount = new ToolStripStatusLabel("件数: 0");
        _statusStrip.Items.AddRange([_lblStatus, _lblCount]);

        // Layout
        Controls.Add(_dgvEmployees);
        Controls.Add(buttonPanel);
        Controls.Add(searchPanel);
        Controls.Add(_menuStrip);
        Controls.Add(_statusStrip);
        MainMenuStrip = _menuStrip;
    }

    private void LoadDepartmentFilter()
    {
        _cmbDepartment.Items.Clear();
        _cmbDepartment.Items.Add("すべて");
        foreach (var dept in _deptRepo.GetAll())
        {
            _cmbDepartment.Items.Add(new DepartmentComboItem(dept.DepartmentId, dept.DepartmentName));
        }
        _cmbDepartment.SelectedIndex = 0;
    }

    private void SearchEmployees()
    {
        try
        {
            var keyword = string.IsNullOrWhiteSpace(_txtSearch.Text) ? null : _txtSearch.Text.Trim();
            int? deptId = _cmbDepartment.SelectedItem is DepartmentComboItem di ? di.Id : null;
            bool? isActive = _cmbStatus.SelectedIndex switch
            {
                1 => true,
                2 => false,
                _ => null
            };

            var employees = _employeeRepo.Search(keyword, deptId, isActive);
            BindGrid(employees);
            _lblCount.Text = $"件数: {employees.Count}";
            _lblStatus.Text = "検索完了";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"検索エラー: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BindGrid(List<Employee> employees)
    {
        _dgvEmployees.DataSource = null;
        _dgvEmployees.Columns.Clear();

        var dt = new System.Data.DataTable();
        dt.Columns.Add("EmployeeId", typeof(int));
        dt.Columns.Add("社員番号", typeof(string));
        dt.Columns.Add("氏名", typeof(string));
        dt.Columns.Add("部署", typeof(string));
        dt.Columns.Add("メール", typeof(string));
        dt.Columns.Add("電話番号", typeof(string));
        dt.Columns.Add("入社日", typeof(string));
        dt.Columns.Add("給与", typeof(string));
        dt.Columns.Add("状態", typeof(string));

        foreach (var e in employees)
        {
            dt.Rows.Add(
                e.EmployeeId,
                e.EmployeeCode,
                e.FullName,
                e.DepartmentName ?? "(未所属)",
                e.Email ?? "",
                e.Phone ?? "",
                e.HireDate.ToString("yyyy/MM/dd"),
                e.Salary.ToString("#,##0"),
                e.IsActive ? "在籍" : "退職"
            );
        }

        _dgvEmployees.DataSource = dt;
        _dgvEmployees.Columns["EmployeeId"]!.Visible = false;
        _dgvEmployees.Columns["社員番号"]!.FillWeight = 10;
        _dgvEmployees.Columns["氏名"]!.FillWeight = 15;
        _dgvEmployees.Columns["部署"]!.FillWeight = 12;
        _dgvEmployees.Columns["メール"]!.FillWeight = 20;
        _dgvEmployees.Columns["電話番号"]!.FillWeight = 13;
        _dgvEmployees.Columns["入社日"]!.FillWeight = 10;
        _dgvEmployees.Columns["給与"]!.FillWeight = 10;
        _dgvEmployees.Columns["給与"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        _dgvEmployees.Columns["状態"]!.FillWeight = 8;
    }

    private int? GetSelectedEmployeeId()
    {
        if (_dgvEmployees.SelectedRows.Count == 0) return null;
        return _dgvEmployees.SelectedRows[0].Cells["EmployeeId"].Value as int?;
    }

    private void AddEmployee()
    {
        using var form = new EmployeeEditForm(_deptRepo);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                _employeeRepo.Insert(form.Employee);
                SearchEmployees();
                _lblStatus.Text = "従業員を追加しました";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"追加エラー: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void EditEmployee()
    {
        var id = GetSelectedEmployeeId();
        if (id == null)
        {
            MessageBox.Show("編集する従業員を選択してください。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var employee = _employeeRepo.GetById(id.Value);
        if (employee == null) return;

        using var form = new EmployeeEditForm(_deptRepo, employee);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                _employeeRepo.Update(form.Employee);
                SearchEmployees();
                _lblStatus.Text = "従業員情報を更新しました";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新エラー: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void DeleteEmployee()
    {
        var id = GetSelectedEmployeeId();
        if (id == null)
        {
            MessageBox.Show("削除する従業員を選択してください。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var emp = _employeeRepo.GetById(id.Value);
        if (emp == null) return;

        var result = MessageBox.Show(
            $"従業員「{emp.FullName}」({emp.EmployeeCode})を削除しますか？\nこの操作は取り消せません。",
            "削除確認", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        if (result == DialogResult.Yes)
        {
            try
            {
                _employeeRepo.Delete(id.Value);
                SearchEmployees();
                _lblStatus.Text = "従業員を削除しました";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"削除エラー: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void ClearSearch()
    {
        _txtSearch.Clear();
        _cmbDepartment.SelectedIndex = 0;
        _cmbStatus.SelectedIndex = 0;
        SearchEmployees();
    }

    private void OpenDepartmentForm()
    {
        using var form = new DepartmentForm(_deptRepo);
        form.ShowDialog(this);
        LoadDepartmentFilter();
        SearchEmployees();
    }

    private record DepartmentComboItem(int Id, string Name)
    {
        public override string ToString() => Name;
    }
}
