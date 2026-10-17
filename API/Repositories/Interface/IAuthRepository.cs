using API.Models;

namespace API.Repositories.Interface;

/// <summary>
/// Data Access abstraction for Authentication and Client Credentials.
/// Encapsulates all ADO.NET SQL operations on Users and ApiClients tables.
/// </summary>
public interface IAuthRepository
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task<ApiClient?> GetActiveApiClientAsync(string clientId, string clientSecret);
}
