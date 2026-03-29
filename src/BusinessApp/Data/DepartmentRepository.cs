using Dapper;
using Microsoft.Data.SqlClient;
using BusinessApp.Models;

namespace BusinessApp.Data;

public class DepartmentRepository
{
    private readonly string _connectionString;

    public DepartmentRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<Department> GetAll()
    {
        using var conn = new SqlConnection(_connectionString);
        return conn.Query<Department>("SELECT * FROM Departments ORDER BY DepartmentCode").ToList();
    }

    public Department? GetById(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        return conn.QueryFirstOrDefault<Department>(
            "SELECT * FROM Departments WHERE DepartmentId = @Id", new { Id = id });
    }

    public int Insert(Department dept)
    {
        using var conn = new SqlConnection(_connectionString);
        return conn.ExecuteScalar<int>(@"
            INSERT INTO Departments (DepartmentCode, DepartmentName)
            VALUES (@DepartmentCode, @DepartmentName);
            SELECT SCOPE_IDENTITY();", dept);
    }

    public void Update(Department dept)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Execute(@"
            UPDATE Departments SET
                DepartmentCode = @DepartmentCode,
                DepartmentName = @DepartmentName,
                UpdatedAt = GETDATE()
            WHERE DepartmentId = @DepartmentId", dept);
    }

    public void Delete(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Execute("DELETE FROM Departments WHERE DepartmentId = @Id", new { Id = id });
    }

    public bool HasEmployees(int departmentId)
    {
        using var conn = new SqlConnection(_connectionString);
        return conn.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Employees WHERE DepartmentId = @Id", new { Id = departmentId }) > 0;
    }
}
