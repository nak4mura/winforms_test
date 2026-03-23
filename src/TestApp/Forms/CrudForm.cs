using System.ComponentModel;
using TestApp.Models;

namespace TestApp.Forms;

public class CrudForm : Form
{
    private readonly DataGridView _grid;
    private readonly TextBox _txtName;
    private readonly TextBox _txtDescription;
    private readonly Button _btnAdd;
    private readonly Button _btnUpdate;
    private readonly Button _btnDelete;
    private readonly Button _btnBack;
    private readonly BindingList<Item> _items = new();
    private int _nextId = 1;

    public CrudForm()
    {
        Text = "データCRUD";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterScreen;

        // DataGridView
        _grid = new DataGridView
        {
            Location = new Point(20, 20),
            Size = new Size(740, 300),
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            DataSource = _items
        };
        _grid.SelectionChanged += Grid_SelectionChanged;

        // Input form
        var lblName = new Label { Text = "名前:", Location = new Point(20, 340), AutoSize = true };
        _txtName = new TextBox { Location = new Point(80, 337), Size = new Size(200, 25) };

        var lblDesc = new Label { Text = "説明:", Location = new Point(20, 375), AutoSize = true };
        _txtDescription = new TextBox { Location = new Point(80, 372), Size = new Size(400, 25) };

        // Buttons
        _btnAdd = new Button { Text = "追加(&A)", Location = new Point(20, 420), Size = new Size(100, 30) };
        _btnAdd.Click += BtnAdd_Click;

        _btnUpdate = new Button { Text = "更新(&U)", Location = new Point(130, 420), Size = new Size(100, 30), Enabled = false };
        _btnUpdate.Click += BtnUpdate_Click;

        _btnDelete = new Button { Text = "削除(&D)", Location = new Point(240, 420), Size = new Size(100, 30), Enabled = false };
        _btnDelete.Click += BtnDelete_Click;

        _btnBack = new Button { Text = "メインへ戻る(&B)", Location = new Point(20, 520), Size = new Size(120, 30) };
        _btnBack.Click += (s, e) => Close();

        Controls.AddRange([_grid, lblName, _txtName, lblDesc, _txtDescription, _btnAdd, _btnUpdate, _btnDelete, _btnBack]);
    }

    private void Grid_SelectionChanged(object? sender, EventArgs e)
    {
        var hasSelection = _grid.SelectedRows.Count > 0;
        _btnUpdate.Enabled = hasSelection;
        _btnDelete.Enabled = hasSelection;

        if (hasSelection && _grid.SelectedRows[0].DataBoundItem is Item item)
        {
            _txtName.Text = item.Name;
            _txtDescription.Text = item.Description;
        }
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtName.Text)) return;

        _items.Add(new Item
        {
            Id = _nextId++,
            Name = _txtName.Text.Trim(),
            Description = _txtDescription.Text.Trim(),
            CreatedAt = DateTime.Now
        });
        ClearInputs();
    }

    private void BtnUpdate_Click(object? sender, EventArgs e)
    {
        if (_grid.SelectedRows.Count == 0 || string.IsNullOrWhiteSpace(_txtName.Text)) return;

        if (_grid.SelectedRows[0].DataBoundItem is Item item)
        {
            var index = _items.IndexOf(item);
            item.Name = _txtName.Text.Trim();
            item.Description = _txtDescription.Text.Trim();
            _items.ResetItem(index);
        }
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (_grid.SelectedRows.Count == 0) return;

        if (_grid.SelectedRows[0].DataBoundItem is Item item)
        {
            _items.Remove(item);
            ClearInputs();
        }
    }

    private void ClearInputs()
    {
        _txtName.Text = string.Empty;
        _txtDescription.Text = string.Empty;
    }
}
