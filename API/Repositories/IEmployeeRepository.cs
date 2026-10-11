using API.DTOs;
using API.Models;

namespace API.Repositories;

public interface IEmployeeRepository
{
    // Contract 1: Fetch employees, optionally filtered by department
    Task<List<Employee>> GetAllAsync(string? department = null);

    // Contract 2: Calculate and return department-wise summary
    Task<List<DepartmentSummaryDto>> GetDepartmentSummaryAsync();
}