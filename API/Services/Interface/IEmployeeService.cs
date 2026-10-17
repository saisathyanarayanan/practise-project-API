using API.DTOs;

namespace API.Services.Interface;

public interface IEmployeeService
{
    // Returns the structured response DTO including metadata and fallback data
    Task<EmployeeResponseDto> GetEmployeesAsync(string? department = null);

    // Returns the calculated department summary
    Task<List<DepartmentSummaryDto>> GetDepartmentSummaryAsync();
}
