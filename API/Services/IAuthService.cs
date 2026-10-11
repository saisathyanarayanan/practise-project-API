using System.Security.Claims;
using API.DTOs;
using API.Models;

namespace API.Services;

/// <summary>
/// Business logic contract for User authentication, claim resolution, and client token issuance.
/// </summary>
public interface IAuthService
{
    Task<string?> LoginAsync(LoginRequest request);
    string RefreshToken(ClaimsPrincipal principal);
    Task<ClientTokenResponseDto?> GenerateClientTokenAsync(ClientTokenRequest request);
}
