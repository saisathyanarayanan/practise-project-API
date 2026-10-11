namespace API.DTOs;

public class DepartmentSummaryDto
{
    public string Department { get; set; } = string.Empty;
    public int TotalEmployees { get; set; }
    public decimal AverageSalary { get; set; }
}
