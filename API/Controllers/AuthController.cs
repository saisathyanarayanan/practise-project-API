using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    // The Controller now depends ONLY on the Service
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _authService.LoginAsync(request);

        if (token == null)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        return Ok(new { token });
    }

    [HttpPost("refresh")]
    [Authorize]
    public IActionResult Refresh()
    {
        var token = _authService.RefreshToken(User);
        return Ok(new { token });
    }

    [HttpPost("client-token")]
    [AllowAnonymous]
    public async Task<IActionResult> GenerateClientToken([FromBody] ClientTokenRequest request)
    {
        var response = await _authService.GenerateClientTokenAsync(request);

        if (response == null)
        {
            return Unauthorized(new { message = "Invalid ClientId or ClientSecret." });
        }

        return Ok(response);
    }
}