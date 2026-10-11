namespace API.DTOs;

public class DepartmentAccessResultDto
{
    public string Status { get; set; } = "Success";
    public string Message { get; set; } = string.Empty;
    public string? Caller { get; set; }
    public string? Department { get; set; }
    public string? CallerDepartment { get; set; }
}
