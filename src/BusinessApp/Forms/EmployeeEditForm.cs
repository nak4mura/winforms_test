using BusinessApp.Data;
using BusinessApp.Models;

namespace BusinessApp.Forms;

public class EmployeeEditForm : Form
{
    private readonly DepartmentRepository _deptRepo;
    private readonly bool _isEdit;

    public Employee Employee { get; private set; }

    private TextBox _txtCode = null!;
    private TextBox _txtLastName = null!;
    private TextBox _txtFirstName = null!;
    private ComboBox _cmbDepartment = null!;
    private TextBox _txtEmail = null!;
    private TextBox _txtPhone = null!;
    private DateTimePicker _dtpHireDate = null!;
    private NumericUpDown _nudSalary = null!;
    private CheckBox _chkIsActive = null!;
    private Button _btnOk = null!;
    private Button _btnCancel = null!;

    public EmployeeEditForm(DepartmentRepository deptRepo, Employee? employee = null)
    {
        _deptRepo = deptRepo;
        _isEdit = employee != null;
        Employee = employee ?? new Employee { HireDate = DateTime.Today, IsActive = true };
        InitializeComponent();
        LoadDepartments();
        if (_isEdit) BindEmployee();
    }

    private void InitializeComponent()
    {
        Text = _isEdit ? "従業員編集" : "従業員追加";
        Size = new Size(480, 440);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;

        var y = 20;
        const int labelX = 20;
        const int inputX = 120;
        const int inputW = 320;
        const int rowH = 35;

        AddLabel("社員番号:", labelX, y);
        _txtCode = AddTextBox(inputX, y, inputW, 10);
        y += rowH;

        AddLabel("姓:", labelX, y);
        _txtLastName = AddTextBox(inputX, y, 140, 50);
        AddLabel("名:", inputX + 160, y);
        _txtFirstName = AddTextBox(inputX + 180, y, 140, 50);
        y += rowH;

        AddLabel("部署:", labelX, y);
        _cmbDepartment = new ComboBox
        {
            Location = new Point(inputX, y), Width = inputW, DropDownStyle = ComboBoxStyle.DropDownList
        };
        Controls.Add(_cmbDepartment);
        y += rowH;

        AddLabel("メール:", labelX, y);
        _txtEmail = AddTextBox(inputX, y, inputW, 256);
        y += rowH;

        AddLabel("電話番号:", labelX, y);
        _txtPhone = AddTextBox(inputX, y, inputW, 20);
        y += rowH;

        AddLabel("入社日:", labelX, y);
        _dtpHireDate = new DateTimePicker
        {
            Location = new Point(inputX, y), Width = inputW, Format = DateTimePickerFormat.Short
        };
        Controls.Add(_dtpHireDate);
        y += rowH;

        AddLabel("給与:", labelX, y);
        _nudSalary = new NumericUpDown
        {
            Location = new Point(inputX, y),
            Width = inputW,
            Maximum = 99999999,
            Minimum = 0,
            DecimalPlaces = 0,
            ThousandsSeparator = true
        };
        Controls.Add(_nudSalary);
        y += rowH;

        _chkIsActive = new CheckBox
        {
            Text = "在籍",
            Location = new Point(inputX, y),
            Checked = true,
            AutoSize = true
        };
        Controls.Add(_chkIsActive);
        y += rowH + 10;

        _btnOk = new Button { Text = "OK", DialogResult = DialogResult.None, Width = 90, Location = new Point(230, y) };
        _btnOk.Click += BtnOk_Click;
        _btnCancel = new Button { Text = "キャンセル", DialogResult = DialogResult.Cancel, Width = 90, Location = new Point(330, y) };

        Controls.AddRange([_btnOk, _btnCancel]);
        AcceptButton = _btnOk;
        CancelButton = _btnCancel;
    }

    private Label AddLabel(string text, int x, int y)
    {
        var lbl = new Label { Text = text, Location = new Point(x, y + 3), AutoSize = true };
        Controls.Add(lbl);
        return lbl;
    }

    private TextBox AddTextBox(int x, int y, int width, int maxLength)
    {
        var txt = new TextBox { Location = new Point(x, y), Width = width, MaxLength = maxLength };
        Controls.Add(txt);
        return txt;
    }

    private void LoadDepartments()
    {
        _cmbDepartment.Items.Clear();
        _cmbDepartment.Items.Add(new DeptItem(null, "(未所属)"));
        foreach (var d in _deptRepo.GetAll())
        {
            _cmbDepartment.Items.Add(new DeptItem(d.DepartmentId, d.DepartmentName));
        }
        _cmbDepartment.SelectedIndex = 0;
    }

    private void BindEmployee()
    {
        _txtCode.Text = Employee.EmployeeCode;
        _txtLastName.Text = Employee.LastName;
        _txtFirstName.Text = Employee.FirstName;
        _txtEmail.Text = Employee.Email;
        _txtPhone.Text = Employee.Phone;
        _dtpHireDate.Value = Employee.HireDate;
        _nudSalary.Value = Employee.Salary;
        _chkIsActive.Checked = Employee.IsActive;

        for (int i = 0; i < _cmbDepartment.Items.Count; i++)
        {
            if (_cmbDepartment.Items[i] is DeptItem di && di.Id == Employee.DepartmentId)
            {
                _cmbDepartment.SelectedIndex = i;
                break;
            }
        }
    }

    private void BtnOk_Click(object? sender, EventArgs e)
    {
        if (!Validate()) return;

        Employee.EmployeeCode = _txtCode.Text.Trim();
        Employee.LastName = _txtLastName.Text.Trim();
        Employee.FirstName = _txtFirstName.Text.Trim();
        Employee.DepartmentId = (_cmbDepartment.SelectedItem as DeptItem)?.Id;
        Employee.Email = string.IsNullOrWhiteSpace(_txtEmail.Text) ? null : _txtEmail.Text.Trim();
        Employee.Phone = string.IsNullOrWhiteSpace(_txtPhone.Text) ? null : _txtPhone.Text.Trim();
        Employee.HireDate = _dtpHireDate.Value.Date;
        Employee.Salary = _nudSalary.Value;
        Employee.IsActive = _chkIsActive.Checked;

        DialogResult = DialogResult.OK;
        Close();
    }

    private new bool Validate()
    {
        if (string.IsNullOrWhiteSpace(_txtCode.Text))
        {
            ShowValidationError("社員番号を入力してください。", _txtCode);
            return false;
        }
        if (string.IsNullOrWhiteSpace(_txtLastName.Text))
        {
            ShowValidationError("姓を入力してください。", _txtLastName);
            return false;
        }
        if (string.IsNullOrWhiteSpace(_txtFirstName.Text))
        {
            ShowValidationError("名を入力してください。", _txtFirstName);
            return false;
        }
        return true;
    }

    private void ShowValidationError(string message, Control control)
    {
        MessageBox.Show(message, "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        control.Focus();
    }

    private record DeptItem(int? Id, string Name)
    {
        public override string ToString() => Name;
    }
}
