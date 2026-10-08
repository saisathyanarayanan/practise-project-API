using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires a valid JWT Bearer token for all actions
public class DepartmentController : ControllerBase
{

    [HttpGet("test")]
    [AllowAnonymous]
    public IActionResult Test()
    {
        return Ok(new
        {
            Service = "Primary API (Port 5012)",
            Message = "Successfully reached through the YARP API Gateway!"
        });
    }

    // ========================================================
    // METHOD A: Accessible ONLY by HR Team
    // ========================================================
    [HttpGet("method-a")]
    [Authorize(Roles = "HR")]
    public IActionResult MethodA()
    {
        var callerId = User.Identity?.Name;
        return Ok(new
        {
            status = "Success",
            message = "Access granted to Method A (Confidential HR Data).",
            caller = callerId,
            department = "HR"
        });
    }

    // ========================================================
    // METHOD B: Accessible ONLY by Finance Team
    // ========================================================
    [HttpGet("method-b")]
    [Authorize(Roles = "Finance")]
    public IActionResult MethodB()
    {
        var callerId = User.Identity?.Name;
        return Ok(new
        {
            status = "Success",
            message = "Access granted to Method B (Confidential Finance Data).",
            caller = callerId,
            department = "Finance"
        });
    }

    // ========================================================
    // METHOD C: Shared Method (Accessible by BOTH HR & Finance)
    // ========================================================
    [HttpGet("shared-method")]
    [Authorize(Roles = "HR,Finance")] // Comma means OR (HR or Finance)
    public IActionResult SharedMethod()
    {
        var callerId = User.Identity?.Name;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new
        {
            status = "Success",
            message = "Access granted to Shared Common Method.",
            caller = callerId,
            callerDepartment = role
        });
    }
}
