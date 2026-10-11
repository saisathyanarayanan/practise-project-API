using API.Models;

namespace API.Repositories;

public interface IUserManagementRepository
{
    Task<int> CreateHashedUserAsync(HashedUser user);
    Task<List<HashedUser>> GetHashedUsersAsync();
    Task<HashedUser?> GetHashedUserByIdAsync(int id);

    Task<int> CreateDirectUserAsync(User user);
    Task<List<User>> GetUsersAsync(bool asMaskedViewer = false);
}
