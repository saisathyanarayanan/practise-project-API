using API.DTOs;
using API.Models;
using API.Repositories;

namespace API.Services;

public class EmployeeService : IEmployeeService
{
    // 1. The Service depends ONLY on the Repository interface (not DbContext or SQL)
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<EmployeeResponseDto> GetEmployeesAsync(string? department = null)
    {
        try
        {
            // Business step 1: Call repository to fetch real data from SQL
            var employees = await _employeeRepository.GetAllAsync(department);

            // Business step 2: Package into the response DTO
            return new EmployeeResponseDto
            {
                Source = "SQL Server Database (via EF Core & LINQ)",
                Count = employees.Count,
                Data = employees
            };
        }
        catch (Exception ex)
        {
            // Business rule: If DB table does not exist or DB is unreachable,
            // fall back to the in-memory sample list for demonstration
            var sampleEmployees = new List<Employee>
            {
                new() { Id = 1, Name = "Sathya N.", Department = "IT", Designation = "Full Stack Architect", Salary = 95000 },
                new() { Id = 2, Name = "Alice Johnson", Department = "HR", Designation = "HR Manager", Salary = 70000 },
                new() { Id = 3, Name = "Robert Vance", Department = "Finance", Designation = "Lead Accountant", Salary = 82000 },
                new() { Id = 4, Name = "David Miller", Department = "IT", Designation = "Cloud Engineer", Salary = 91000 }
            };

            var filtered = sampleEmployees
                .Where(e => string.IsNullOrWhiteSpace(department) || e.Department.Equals(department, StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.Name)
                .ToList();

            return new EmployeeResponseDto
            {
                Source = "LINQ Demonstration (In-Memory Fallback)",
                Note = "To sync with real database, run 'dotnet ef database update'",
                DbStatus = ex.Message,
                Count = filtered.Count,
                Data = filtered
            };
        }
    }

    public async Task<List<DepartmentSummaryDto>> GetDepartmentSummaryAsync()
    {
        try
        {
            return await _employeeRepository.GetDepartmentSummaryAsync();
        }
        catch
        {
            // Fallback summary if DB is offline
            return new List<DepartmentSummaryDto>
            {
                new() { Department = "IT", TotalEmployees = 2, AverageSalary = 93000.00m },
                new() { Department = "HR", TotalEmployees = 1, AverageSalary = 70000.00m },
                new() { Department = "Finance", TotalEmployees = 1, AverageSalary = 82000.00m }
            };
        }
    }
}