using API.Models;

namespace API.DTOs;

public class EmployeeResponseDto
{
    public string Source { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string? DbStatus { get; set; }
    public int Count { get; set; }
    public List<Employee> Data { get; set; } = new();
}
