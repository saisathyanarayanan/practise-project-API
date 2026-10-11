using API.DTOs;
using API.Models;

namespace API.Services;

public interface IUserManagementService
{
    Task<HashedUser> CreateHashedUserAsync(CreateHashedUserDto dto);
    Task<List<HashedUser>> GetHashedUsersAsync();
    Task<VerifyPasswordResultDto> VerifyPasswordAsync(VerifyPasswordDto dto);

    Task<User> CreateDirectUserAsync(CreateDirectUserDto dto);
    Task<List<User>> GetUsersAsync(bool asMaskedViewer = false);
}
