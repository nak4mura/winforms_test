using System.ComponentModel;
using System.Data;
using Microsoft.Data.SqlClient;
using TestApp.Models;

namespace TestApp.Forms;

public class DbCrudForm : Form
{
    private const string ConnectionString =
        @"Data Source=(localdb)\MSSQLLocalDB;Database=TestDb;Integrated Security=true;TrustServerCertificate=true;";

    private readonly DataGridView _grid;
    private readonly TextBox _txtName;
    private readonly TextBox _txtDescription;
    private readonly Button _btnAdd;
    private readonly Button _btnUpdate;
    private readonly Button _btnDelete;
    private readonly Button _btnBack;
    private readonly BindingList<Item> _items = new();

    public DbCrudForm()
    {
        Text = "データCRUD (DB)";
        Name = "DbCrudForm";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterScreen;

        // DataGridView
        _grid = new DataGridView
        {
            Name = "DbCrudDataGrid",
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
        _txtName = new TextBox { Name = "TxtDbItemName", Location = new Point(80, 337), Size = new Size(200, 25) };

        var lblDesc = new Label { Text = "説明:", Location = new Point(20, 375), AutoSize = true };
        _txtDescription = new TextBox { Name = "TxtDbItemDescription", Location = new Point(80, 372), Size = new Size(400, 25) };

        // Buttons
        _btnAdd = new Button { Name = "BtnDbAdd", Text = "追加(&A)", Location = new Point(20, 420), Size = new Size(100, 30) };
        _btnAdd.Click += BtnAdd_Click;

        _btnUpdate = new Button { Name = "BtnDbUpdate", Text = "更新(&U)", Location = new Point(130, 420), Size = new Size(100, 30), Enabled = false };
        _btnUpdate.Click += BtnUpdate_Click;

        _btnDelete = new Button { Name = "BtnDbDelete", Text = "削除(&D)", Location = new Point(240, 420), Size = new Size(100, 30), Enabled = false };
        _btnDelete.Click += BtnDelete_Click;

        _btnBack = new Button { Name = "BtnDbCrudBack", Text = "メインへ戻る(&B)", Location = new Point(20, 520), Size = new Size(120, 30) };
        _btnBack.Click += (s, e) => Close();

        Controls.AddRange([_grid, lblName, _txtName, lblDesc, _txtDescription, _btnAdd, _btnUpdate, _btnDelete, _btnBack]);

        Load += (s, e) => LoadData();
    }

    private void LoadData()
    {
        _items.Clear();
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Description, CreatedAt FROM Items ORDER BY Id";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            _items.Add(new Item
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                CreatedAt = reader.GetDateTime(3)
            });
        }
        ClearInputs();
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

        using var conn = new SqlConnection(ConnectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Items (Name, Description, CreatedAt) VALUES (@Name, @Description, @CreatedAt)";
        cmd.Parameters.AddWithValue("@Name", _txtName.Text.Trim());
        cmd.Parameters.AddWithValue("@Description", _txtDescription.Text.Trim());
        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
        cmd.ExecuteNonQuery();

        LoadData();
    }

    private void BtnUpdate_Click(object? sender, EventArgs e)
    {
        if (_grid.SelectedRows.Count == 0 || string.IsNullOrWhiteSpace(_txtName.Text)) return;
        if (_grid.SelectedRows[0].DataBoundItem is not Item item) return;

        using var conn = new SqlConnection(ConnectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Items SET Name = @Name, Description = @Description WHERE Id = @Id";
        cmd.Parameters.AddWithValue("@Name", _txtName.Text.Trim());
        cmd.Parameters.AddWithValue("@Description", _txtDescription.Text.Trim());
        cmd.Parameters.AddWithValue("@Id", item.Id);
        cmd.ExecuteNonQuery();

        LoadData();
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (_grid.SelectedRows.Count == 0) return;
        if (_grid.SelectedRows[0].DataBoundItem is not Item item) return;

        using var conn = new SqlConnection(ConnectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Items WHERE Id = @Id";
        cmd.Parameters.AddWithValue("@Id", item.Id);
        cmd.ExecuteNonQuery();

        LoadData();
    }

    private void ClearInputs()
    {
        _txtName.Text = string.Empty;
        _txtDescription.Text = string.Empty;
    }
}
