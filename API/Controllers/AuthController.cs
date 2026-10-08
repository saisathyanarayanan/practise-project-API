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

    // =======================================================================
    // ADO.NET: Token Generation for External Teams (HR / Finance)
    // =======================================================================
    [HttpPost("client-token")]
    [AllowAnonymous]
    public IActionResult GenerateClientToken([FromBody] ClientTokenRequest request)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection")!;

        using var connection = new SqlConnection(connectionString);
        connection.Open();

        var query = @"
            SELECT Id, ClientId, ClientSecret, Department, IsActive 
            FROM ApiClients 
            WHERE ClientId = @ClientId AND ClientSecret = @ClientSecret AND IsActive = 1";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ClientId", request.ClientId);
        command.Parameters.AddWithValue("@ClientSecret", request.ClientSecret);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
            return Unauthorized(new { message = "Invalid ClientId or ClientSecret." });

        var client = new ApiClient
        {
            Id = (int)reader["Id"],
            ClientId = reader["ClientId"].ToString()!,
            ClientSecret = reader["ClientSecret"].ToString()!,
            Department = reader["Department"].ToString()!,
            IsActive = (bool)reader["IsActive"]
        };

        var token = JwtExtensions.GenerateClientToken(_jwtSettings, client);

        return Ok(new
        {
            access_token = token,
            token_type = "Bearer",
            expires_in = _jwtSettings.ExpiryInMinutes * 60,
            department = client.Department
        });
    }
}