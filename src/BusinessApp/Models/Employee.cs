namespace BusinessApp.Models;

public class Employee
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string FullName => $"{LastName} {FirstName}";
}
