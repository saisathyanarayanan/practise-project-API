using System.Security.Claims;
using API.DTOs;
using API.Jwt;
using API.Models;
using API.Repositories;

namespace API.Services;

/// <summary>
/// Business logic implementation for Authentication.
/// Orchestrates data access via IAuthRepository and token generation via JwtExtensions.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly JwtSettings _jwtSettings;

    public AuthService(IAuthRepository authRepository, JwtSettings jwtSettings)
    {
        _authRepository = authRepository;
        _jwtSettings = jwtSettings;
    }

    public async Task<string?> LoginAsync(LoginRequest request)
    {
        // 1. Fetch user from database via ADO.NET repository
        var user = await _authRepository.GetUserByUsernameAsync(request.Username);

        // 2. Business rule: verify existence and password match
        if (user == null || user.Password != request.Password)
        {
            return null; // Authentication failed
        }

        // 3. Issue JWT token
        return JwtExtensions.GenerateToken(_jwtSettings, user);
    }

    public string RefreshToken(ClaimsPrincipal principal)
    {
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = principal.FindFirst(ClaimTypes.Name)?.Value;
        var role = principal.FindFirst(ClaimTypes.Role)?.Value;

        var user = new User
        {
            Id = int.Parse(userId!),
            Username = username!,
            Role = role!
        };

        return JwtExtensions.GenerateToken(_jwtSettings, user);
    }

    public async Task<ClientTokenResponseDto?> GenerateClientTokenAsync(ClientTokenRequest request)
    {
        // 1. Fetch active client credentials from database via ADO.NET repository
        var client = await _authRepository.GetActiveApiClientAsync(request.ClientId, request.ClientSecret);

        // 2. Business rule: verify client exists and is active
        if (client == null)
        {
            return null; // Invalid credentials or inactive client
        }

        // 3. Generate JWT bearer token with department claims
        var token = JwtExtensions.GenerateClientToken(_jwtSettings, client);

        return new ClientTokenResponseDto
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = _jwtSettings.ExpiryInMinutes * 60,
            Department = client.Department
        };
    }
}
