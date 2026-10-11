using API.DTOs;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Accessible for testing the two security forms
public class UserManagementController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public UserManagementController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    /// <summary>
    /// Form 1 Endpoint: Hashes password using Argon2id + cryptographically random salt,
    /// stores into maskedpasssword_usersTable, and returns the persisted record.
    /// </summary>
    [HttpPost("hashed")]
    public async Task<IActionResult> CreateHashedUser([FromBody] CreateHashedUserDto dto)
    {
        try
        {
            var result = await _userManagementService.CreateHashedUserAsync(dto);
            return Ok(new
            {
                message = "User registered successfully with Argon2id password hashing & salt.",
                user = result
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Form 1 Endpoint: Retrieves all records from maskedpasssword_usersTable.
    /// </summary>
    [HttpGet("hashed")]
    public async Task<IActionResult> GetHashedUsers()
    {
        var users = await _userManagementService.GetHashedUsersAsync();
        return Ok(users);
    }

    /// <summary>
    /// Form 1 Endpoint: Demonstrates enterprise password verification against Argon2id hash.
    /// Since cryptographic hashes cannot be decrypted, enterprise systems verify candidates by constant-time re-hashing.
    /// </summary>
    [HttpPost("hashed/verify")]
    public async Task<IActionResult> VerifyHashedPassword([FromBody] VerifyPasswordDto dto)
    {
        var result = await _userManagementService.VerifyPasswordAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Form 2 Endpoint: Directly saves password without encryption into the 'Users' table.
    /// The password column is protected in SQL Server via Dynamic Data Masking (DDM).
    /// </summary>
    [HttpPost("direct")]
    public async Task<IActionResult> CreateDirectUser([FromBody] CreateDirectUserDto dto)
    {
        try
        {
            var result = await _userManagementService.CreateDirectUserAsync(dto);
            return Ok(new
            {
                message = "User registered directly in 'Users' table (protected by SQL Server Dynamic Data Masking).",
                user = result
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Form 2 Endpoint: Retrieves records from 'Users'.
    /// When asMaskedViewer is true, simulates a non-privileged query showing SQL Server DDM (xxxx).
    /// When asMaskedViewer is false, shows the raw underlying values.
    /// </summary>
    [HttpGet("direct")]
    public async Task<IActionResult> GetDirectUsers([FromQuery] bool asMaskedViewer = false)
    {
        var users = await _userManagementService.GetUsersAsync(asMaskedViewer);
        return Ok(new
        {
            viewMode = asMaskedViewer ? "SQL Server Dynamic Data Masked (Non-privileged User)" : "Admin Unmasked View",
            count = users.Count,
            users
        });
    }
}
