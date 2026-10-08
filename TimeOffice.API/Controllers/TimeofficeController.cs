using Microsoft.AspNetCore.Mvc;

namespace TimeOffice.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TimeofficeController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAttendance()
    {
        return Ok(new
        {
            Service = "TimeOffice Service",
            Timestamp = DateTime.UtcNow,
            Data = new[]
            {
                new { EmployeeId = "EMP101", Name = "Alice", Status = "Present", ClockIn = "09:02 AM" },
                new { EmployeeId = "EMP102", Name = "Bob", Status = "Present", ClockIn = "08:55 AM" }
            }
        });
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new { Status = "TimeOffice API is healthy and running!" });
    }
}