using BusinessApp.Data;
using BusinessApp.Models;

namespace BusinessApp.Forms;

public class DepartmentForm : Form
{
    private readonly DepartmentRepository _deptRepo;

    private DataGridView _dgv = null!;
    private TextBox _txtCode = null!;
    private TextBox _txtName = null!;
    private Button _btnAdd = null!;
    private Button _btnUpdate = null!;
    private Button _btnDelete = null!;
    private Button _btnClose = null!;

    private int? _selectedId;

    public DepartmentForm(DepartmentRepository deptRepo)
    {
        _deptRepo = deptRepo;
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        Text = "部署マスタ管理";
        Size = new Size(600, 450);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;

        _dgv = new DataGridView
        {
            Location = new Point(12, 12),
            Size = new Size(560, 250),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = SystemColors.Window,
            RowHeadersVisible = false
        };
        _dgv.SelectionChanged += Dgv_SelectionChanged;

        var editPanel = new GroupBox
        {
            Text = "部署情報",
            Location = new Point(12, 270),
            Size = new Size(560, 90)
        };

        var lblCode = new Label { Text = "コード:", Location = new Point(15, 30), AutoSize = true };
        _txtCode = new TextBox { Location = new Point(70, 27), Width = 100, MaxLength = 10 };
        var lblName = new Label { Text = "部署名:", Location = new Point(190, 30), AutoSize = true };
        _txtName = new TextBox { Location = new Point(250, 27), Width = 200, MaxLength = 100 };

        _btnAdd = new Button { Text = "追加", Location = new Point(15, 58), Width = 75 };
        _btnAdd.Click += (_, _) => AddDepartment();
        _btnUpdate = new Button { Text = "更新", Location = new Point(100, 58), Width = 75 };
        _btnUpdate.Click += (_, _) => UpdateDepartment();
        _btnDelete = new Button { Text = "削除", Location = new Point(185, 58), Width = 75 };
        _btnDelete.Click += (_, _) => DeleteDepartment();

        editPanel.Controls.AddRange([lblCode, _txtCode, lblName, _txtName, _btnAdd, _btnUpdate, _btnDelete]);

        _btnClose = new Button { Text = "閉じる", Location = new Point(497, 375), Width = 75, DialogResult = DialogResult.Cancel };

        Controls.AddRange([_dgv, editPanel, _btnClose]);
        CancelButton = _btnClose;
    }

    private void LoadData()
    {
        var depts = _deptRepo.GetAll();
        var dt = new System.Data.DataTable();
        dt.Columns.Add("DepartmentId", typeof(int));
        dt.Columns.Add("コード", typeof(string));
        dt.Columns.Add("部署名", typeof(string));
        dt.Columns.Add("作成日", typeof(string));

        foreach (var d in depts)
        {
            dt.Rows.Add(d.DepartmentId, d.DepartmentCode, d.DepartmentName, d.CreatedAt.ToString("yyyy/MM/dd"));
        }

        _dgv.DataSource = dt;
        _dgv.Columns["DepartmentId"]!.Visible = false;
        _selectedId = null;
        ClearInputs();
    }

    private void Dgv_SelectionChanged(object? sender, EventArgs e)
    {
        if (_dgv.SelectedRows.Count == 0) return;
        var row = _dgv.SelectedRows[0];
        _selectedId = row.Cells["DepartmentId"].Value as int? ?? 0;
        _txtCode.Text = row.Cells["コード"].Value?.ToString();
        _txtName.Text = row.Cells["部署名"].Value?.ToString();
    }

    private void AddDepartment()
    {
        if (!ValidateInput()) return;
        try
        {
            _deptRepo.Insert(new Department
            {
                DepartmentCode = _txtCode.Text.Trim(),
                DepartmentName = _txtName.Text.Trim()
            });
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"追加エラー: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateDepartment()
    {
        if (_selectedId == null)
        {
            MessageBox.Show("更新する部署を選択してください。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (!ValidateInput()) return;
        try
        {
            _deptRepo.Update(new Department
            {
                DepartmentId = _selectedId.Value,
                DepartmentCode = _txtCode.Text.Trim(),
                DepartmentName = _txtName.Text.Trim()
            });
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"更新エラー: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DeleteDepartment()
    {
        if (_selectedId == null)
        {
            MessageBox.Show("削除する部署を選択してください。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (_deptRepo.HasEmployees(_selectedId.Value))
        {
            MessageBox.Show("この部署には所属する従業員がいるため削除できません。", "削除不可",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var result = MessageBox.Show("選択した部署を削除しますか？", "削除確認",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (result == DialogResult.Yes)
        {
            try
            {
                _deptRepo.Delete(_selectedId.Value);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"削除エラー: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(_txtCode.Text))
        {
            MessageBox.Show("コードを入力してください。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtCode.Focus();
            return false;
        }
        if (string.IsNullOrWhiteSpace(_txtName.Text))
        {
            MessageBox.Show("部署名を入力してください。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtName.Focus();
            return false;
        }
        return true;
    }

    private void ClearInputs()
    {
        _txtCode.Clear();
        _txtName.Clear();
    }
}
