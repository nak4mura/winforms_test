using Microsoft.Data.SqlClient;

namespace BusinessApp.Data;

public static class DatabaseInitializer
{
    private const string MasterConnectionString =
        @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

    public static void Initialize(string connectionString)
    {
        EnsureDatabase(connectionString);
        EnsureTables(connectionString);
        SeedData(connectionString);
    }

    private static void EnsureDatabase(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var dbName = builder.InitialCatalog;
        if (string.IsNullOrEmpty(dbName))
            dbName = "BusinessAppDb";

        using var conn = new SqlConnection(MasterConnectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $@"
            IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = @dbName)
            BEGIN
                CREATE DATABASE [{dbName}]
            END";
        cmd.Parameters.AddWithValue("@dbName", dbName);
        cmd.ExecuteNonQuery();
    }

    private static void EnsureTables(string connectionString)
    {
        using var conn = new SqlConnection(connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Departments')
            BEGIN
                CREATE TABLE Departments (
                    DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
                    DepartmentCode NVARCHAR(10) NOT NULL UNIQUE,
                    DepartmentName NVARCHAR(100) NOT NULL,
                    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
                    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
                )
            END;

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees')
            BEGIN
                CREATE TABLE Employees (
                    EmployeeId INT IDENTITY(1,1) PRIMARY KEY,
                    EmployeeCode NVARCHAR(10) NOT NULL UNIQUE,
                    LastName NVARCHAR(50) NOT NULL,
                    FirstName NVARCHAR(50) NOT NULL,
                    DepartmentId INT NULL REFERENCES Departments(DepartmentId),
                    Email NVARCHAR(256) NULL,
                    Phone NVARCHAR(20) NULL,
                    HireDate DATE NOT NULL,
                    Salary DECIMAL(12,2) NOT NULL DEFAULT 0,
                    IsActive BIT NOT NULL DEFAULT 1,
                    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
                    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
                )
            END";
        cmd.ExecuteNonQuery();
    }

    private static void SeedData(string connectionString)
    {
        using var conn = new SqlConnection(connectionString);
        conn.Open();

        using var check = conn.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM Departments";
        var count = (int)check.ExecuteScalar()!;
        if (count > 0) return;

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Departments (DepartmentCode, DepartmentName) VALUES
            ('SALES', N'営業部'),
            ('DEV', N'開発部'),
            ('HR', N'人事部'),
            ('ACC', N'経理部'),
            ('GA', N'総務部');

            INSERT INTO Employees (EmployeeCode, LastName, FirstName, DepartmentId, Email, Phone, HireDate, Salary, IsActive) VALUES
            ('EMP001', N'田中', N'太郎', 1, 'tanaka@example.com', '03-1234-5678', '2020-04-01', 350000, 1),
            ('EMP002', N'鈴木', N'花子', 2, 'suzuki@example.com', '03-2345-6789', '2019-04-01', 420000, 1),
            ('EMP003', N'佐藤', N'一郎', 2, 'sato@example.com', '03-3456-7890', '2021-10-01', 380000, 1),
            ('EMP004', N'高橋', N'美咲', 3, 'takahashi@example.com', '03-4567-8901', '2018-04-01', 450000, 1),
            ('EMP005', N'伊藤', N'健太', 4, 'ito@example.com', '03-5678-9012', '2022-04-01', 320000, 1),
            ('EMP006', N'渡辺', N'さくら', 1, 'watanabe@example.com', '03-6789-0123', '2023-04-01', 300000, 1),
            ('EMP007', N'山本', N'大輔', 5, 'yamamoto@example.com', '03-7890-1234', '2017-04-01', 480000, 1),
            ('EMP008', N'中村', N'愛', 2, 'nakamura@example.com', '03-8901-2345', '2021-04-01', 390000, 1),
            ('EMP009', N'小林', N'翔', 1, 'kobayashi@example.com', '03-9012-3456', '2020-10-01', 360000, 0),
            ('EMP010', N'加藤', N'陽子', 3, 'kato@example.com', '03-0123-4567', '2024-04-01', 280000, 1)";
        cmd.ExecuteNonQuery();
    }
}
