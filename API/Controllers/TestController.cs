using API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IDatabaseHealthService _healthService;

    // Inject only IDatabaseHealthService (Zero direct DbContext or SqlConnection!)
    public TestController(IDatabaseHealthService healthService)
    {
        _healthService = healthService;
    }

    // ==========================================
    // 1. EF CORE APPROACH
    // ==========================================
    [HttpGet("check-efcore")]
    public IActionResult CheckEfCore()
    {
        bool canConnect = _healthService.CheckEfCore();

        if (canConnect)
        {
            return Ok("EF Core: Successfully connected to the 'Project' database!");
        }

        return StatusCode(500, "EF Core: Could not connect to the database.");
    }

    // ==========================================
    // 2. ADO.NET APPROACH (Raw SQL)
    // ==========================================
    [HttpGet("check-adonet")]
    public async Task<IActionResult> CheckAdoNet()
    {
        var result = await _healthService.CheckAdoNetAsync();

        if (result.IsSuccess)
        {
            return Ok(new
            {
                result.Approach,
                result.Status,
                result.DatabaseName,
                result.ConnectionState
            });
        }

        return StatusCode(500, $"ADO.NET Connection Failed: {result.ErrorMessage}");
    }

    // ==========================================
    // Existing test methods
    // ==========================================
    [HttpGet("method-one")]
    public ActionResult<string> GetMethodOne()
    {
        return Ok("METHOD ONE");
    }

    [HttpGet("method-two")]
    [HttpGet("method2")]
    public ActionResult<string> GetMethodTwo()
    {
        return Ok("METHOD 2");
    }
}