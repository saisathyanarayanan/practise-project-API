using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly AppDbContext _context;

    // Constructor: ASP.NET Core automatically injects the AppDbContext instance here
    public EmployeeController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/Employee?department=IT
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetEmployees([FromQuery] string? department = null)
    {
        try
        {
            // LINQ QUERY APPROACH:
            // 1. Start with IQueryable (does not execute SQL yet)
            IQueryable<Employee> query = _context.Employees;

            // 2. LINQ .Where() -> Translates to SQL "WHERE Department = @dept"
            if (!string.IsNullOrWhiteSpace(department))
            {
                query = query.Where(e => e.Department.ToLower() == department.ToLower());
            }

            // 3. LINQ .OrderBy() -> Translates to SQL "ORDER BY Name ASC"
            // 4. LINQ .ToListAsync() -> Executes the SQL query and returns results asynchronously
            var list = await query
                .OrderBy(e => e.Name)
                .ToListAsync();

            return Ok(new
            {
                source = "SQL Server Database (via EF Core & LINQ)",
                count = list.Count,
                data = list
            });
        }
        catch (Exception ex)
        {
            // Fallback sample data if database table has not been created yet
            var sampleEmployees = new List<Employee>
            {
                new() { Id = 1, Name = "Sathya N.", Department = "IT", Designation = "Full Stack Architect", Salary = 95000 },
                new() { Id = 2, Name = "Alice Johnson", Department = "HR", Designation = "HR Manager", Salary = 70000 },
                new() { Id = 3, Name = "Robert Vance", Department = "Finance", Designation = "Lead Accountant", Salary = 82000 },
                new() { Id = 4, Name = "David Miller", Department = "IT", Designation = "Cloud Engineer", Salary = 91000 }
            };

            // In-Memory LINQ demonstration
            var filtered = sampleEmployees
                .Where(e => string.IsNullOrWhiteSpace(department) || e.Department.Equals(department, StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.Name)
                .ToList();

            return Ok(new
            {
                source = "LINQ Demonstration (In-Memory Fallback)",
                note = "To sync with real database, run 'dotnet ef database update'",
                dbStatus = ex.Message,
                count = filtered.Count,
                data = filtered
            });
        }
    }

    // GET /api/Employee/summary (LINQ GroupBy & Aggregations)
    [HttpGet("summary")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDepartmentSummary()
    {
        try
        {
            // LINQ .GroupBy() + Aggregations -> Translates to SQL "GROUP BY Department, COUNT(*), AVG(Salary)"
            var summary = await _context.Employees
                .GroupBy(e => e.Department)
                .Select(g => new
                {
                    Department = g.Key,
                    TotalEmployees = g.Count(),
                    AverageSalary = g.Average(e => e.Salary)
                })
                .ToListAsync();

            return Ok(summary);
        }
        catch
        {
            return Ok(new[]
            {
                new { Department = "IT", TotalEmployees = 2, AverageSalary = 93000.00 },
                new { Department = "HR", TotalEmployees = 1, AverageSalary = 70000.00 },
                new { Department = "Finance", TotalEmployees = 1, AverageSalary = 82000.00 }
            });
        }
    }
}
