using API.DTOs;
using API.Models;
using API.Repositories;

namespace API.Services;

public class UserManagementService : IUserManagementService
{
    private readonly IUserManagementRepository _repository;
    private readonly IArgon2PasswordService _argon2PasswordService;

    public UserManagementService(
        IUserManagementRepository repository,
        IArgon2PasswordService argon2PasswordService)
    {
        _repository = repository;
        _argon2PasswordService = argon2PasswordService;
    }

    public async Task<HashedUser> CreateHashedUserAsync(CreateHashedUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ArgumentException("Username and Password are required.");
        }

        // 1. Hash password with Argon2id + cryptographically random salt (OWASP enterprise pattern)
        var (hashString, saltBase64) = _argon2PasswordService.HashPassword(dto.Password);

        var hashedUser = new HashedUser
        {
            Username = dto.Username.Trim(),
            PasswordHash = hashString,
            Salt = saltBase64,
            Role = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        // 2. Persist to maskedpasssword_usersTable
        int id = await _repository.CreateHashedUserAsync(hashedUser);
        hashedUser.Id = id;

        return hashedUser;
    }

    public async Task<List<HashedUser>> GetHashedUsersAsync()
    {
        return await _repository.GetHashedUsersAsync();
    }

    public async Task<VerifyPasswordResultDto> VerifyPasswordAsync(VerifyPasswordDto dto)
    {
        var user = await _repository.GetHashedUserByIdAsync(dto.Id);
        if (user == null)
        {
            return new VerifyPasswordResultDto
            {
                IsMatch = false,
                Message = "User not found.",
                Explanation = "The specified user ID does not exist in maskedpasssword_usersTable."
            };
        }

        // Argon2id verification
        bool isMatch = _argon2PasswordService.VerifyPassword(user.PasswordHash, dto.CandidatePassword);

        return new VerifyPasswordResultDto
        {
            IsMatch = isMatch,
            Message = isMatch ? "Password verified successfully!" : "Password does not match.",
            Explanation = "Argon2id is a one-way cryptographic hash function. Enterprise systems never decrypt stored hashes; they re-hash the candidate password using the salt and parameters embedded in the PHC string and check equality."
        };
    }

    public async Task<User> CreateDirectUserAsync(CreateDirectUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ArgumentException("Username and Password are required.");
        }

        var user = new User
        {
            Username = dto.Username.Trim(),
            Password = dto.Password, // Saved directly as raw text; masked via SQL Server Dynamic Data Masking
            Role = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role.Trim()
        };

        int id = await _repository.CreateDirectUserAsync(user);
        user.Id = id;

        return user;
    }

    public async Task<List<User>> GetUsersAsync(bool asMaskedViewer = false)
    {
        return await _repository.GetUsersAsync(asMaskedViewer);
    }
}
