using System.Security.Claims;
using API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires a valid JWT Bearer token for all actions
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet("test")]
    [AllowAnonymous]
    public IActionResult Test()
    {
        var result = _departmentService.GetGatewayStatus();
        return Ok(result);
    }

    // ========================================================
    // METHOD A: Accessible ONLY by HR Team
    // ========================================================
    [HttpGet("method-a")]
    [Authorize(Roles = "HR")]
    public IActionResult MethodA()
    {
        var callerId = User.Identity?.Name;
        var result = _departmentService.GetHrAccessData(callerId);
        return Ok(result);
    }

    // ========================================================
    // METHOD B: Accessible ONLY by Finance Team
    // ========================================================
    [HttpGet("method-b")]
    [Authorize(Roles = "Finance")]
    public IActionResult MethodB()
    {
        var callerId = User.Identity?.Name;
        var result = _departmentService.GetFinanceAccessData(callerId);
        return Ok(result);
    }

    // ========================================================
    // METHOD C: Shared Method (Accessible by BOTH HR & Finance)
    // ========================================================
    [HttpGet("shared-method")]
    [Authorize(Roles = "HR,Finance")] // HR or Finance
    public IActionResult SharedMethod()
    {
        var callerId = User.Identity?.Name;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var result = _departmentService.GetSharedAccessData(callerId, role);
        return Ok(result);
    }
}
