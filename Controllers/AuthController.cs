using API.Jwt;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly JwtSettings _jwtSettings;

    public AuthController(IConfiguration configuration, JwtSettings jwtSettings)
    {
        _configuration = configuration;
        _jwtSettings = jwtSettings;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection")!;

        using var connection = new SqlConnection(connectionString);
        connection.Open();

        var query = "SELECT Id, Username, Password, Role FROM Users WHERE Username = @Username";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Username", request.Username);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
            return Unauthorized(new { message = "Invalid username or password." });

        var user = new User
        {
            Id = (int)reader["Id"],
            Username = reader["Username"].ToString()!,
            Password = reader["Password"].ToString()!,
            Role = reader["Role"].ToString()!
        };

        if (user.Password != request.Password)
            return Unauthorized(new { message = "Invalid username or password." });

        var token = JwtExtensions.GenerateToken(_jwtSettings, user);
        return Ok(new { token });
    }

    [HttpPost("refresh")]
    [Authorize]
    public IActionResult Refresh()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        var user = new User
        {
            Id = int.Parse(userId!),
            Username = username!,
            Role = role!
        };

        var token = JwtExtensions.GenerateToken(_jwtSettings, user);
        return Ok(new { token });
    }
}