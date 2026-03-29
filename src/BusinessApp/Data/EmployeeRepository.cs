using Dapper;
using Microsoft.Data.SqlClient;
using BusinessApp.Models;

namespace BusinessApp.Data;

public class EmployeeRepository
{
    private readonly string _connectionString;

    public EmployeeRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<Employee> Search(string? keyword, int? departmentId, bool? isActive)
    {
        using var conn = new SqlConnection(_connectionString);
        var sql = @"
            SELECT e.*, d.DepartmentName
            FROM Employees e
            LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
            WHERE 1=1";
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            sql += @" AND (e.EmployeeCode LIKE @Keyword
                       OR e.LastName LIKE @Keyword
                       OR e.FirstName LIKE @Keyword
                       OR e.Email LIKE @Keyword)";
            parameters.Add("Keyword", $"%{keyword}%");
        }
        if (departmentId.HasValue)
        {
            sql += " AND e.DepartmentId = @DepartmentId";
            parameters.Add("DepartmentId", departmentId.Value);
        }
        if (isActive.HasValue)
        {
            sql += " AND e.IsActive = @IsActive";
            parameters.Add("IsActive", isActive.Value);
        }

        sql += " ORDER BY e.EmployeeCode";
        return conn.Query<Employee>(sql, parameters).ToList();
    }

    public Employee? GetById(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        return conn.QueryFirstOrDefault<Employee>(
            @"SELECT e.*, d.DepartmentName
              FROM Employees e
              LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
              WHERE e.EmployeeId = @Id", new { Id = id });
    }

    public int Insert(Employee emp)
    {
        using var conn = new SqlConnection(_connectionString);
        return conn.ExecuteScalar<int>(@"
            INSERT INTO Employees (EmployeeCode, LastName, FirstName, DepartmentId, Email, Phone, HireDate, Salary, IsActive)
            VALUES (@EmployeeCode, @LastName, @FirstName, @DepartmentId, @Email, @Phone, @HireDate, @Salary, @IsActive);
            SELECT SCOPE_IDENTITY();", emp);
    }

    public void Update(Employee emp)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Execute(@"
            UPDATE Employees SET
                EmployeeCode = @EmployeeCode,
                LastName = @LastName,
                FirstName = @FirstName,
                DepartmentId = @DepartmentId,
                Email = @Email,
                Phone = @Phone,
                HireDate = @HireDate,
                Salary = @Salary,
                IsActive = @IsActive,
                UpdatedAt = GETDATE()
            WHERE EmployeeId = @EmployeeId", emp);
    }

    public void Delete(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Execute("DELETE FROM Employees WHERE EmployeeId = @Id", new { Id = id });
    }
}
