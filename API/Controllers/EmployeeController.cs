using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    // 1. Controller only depends on the Service abstraction
    private readonly IEmployeeService _employeeService;

    // 2. ASP.NET Core Dependency Injection provides the service instance
    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    // GET /api/Employee?department=IT
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetEmployees([FromQuery] string? department = null)
    {
        var result = await _employeeService.GetEmployeesAsync(department);
        return Ok(result);
    }

    // GET /api/Employee/summary
    [HttpGet("summary")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDepartmentSummary()
    {
        var summary = await _employeeService.GetDepartmentSummaryAsync();
        return Ok(summary);
    }
}