using API.Data;
using API.DTOs;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    // 1. Private field holding the database context
    private readonly AppDbContext _context;

    // 2. Constructor Injection: ASP.NET Core automatically supplies AppDbContext
    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Employee>> GetAllAsync(string? department = null)                // GetEmployees
    {
        // Start building the query (IQueryable doesn't hit SQL Server yet)
        IQueryable<Employee> query = _context.Employees;

        if (!string.IsNullOrWhiteSpace(department))
        {
            query = query.Where(e => e.Department.ToLower() == department.ToLower());
        }

        // ToListAsync() sends the compiled SQL SELECT query to SQL Server
        return await query
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<List<DepartmentSummaryDto>> GetDepartmentSummaryAsync()               // GetDepartmentSummary
    {
        // EF Core translates this directly to SQL "GROUP BY Department" with COUNT and AVG
        return await _context.Employees
            .GroupBy(e => e.Department)
            .Select(g => new DepartmentSummaryDto
            {
                Department = g.Key,
                TotalEmployees = g.Count(),
                AverageSalary = g.Average(e => e.Salary)
            })
            .ToListAsync();
    }
}