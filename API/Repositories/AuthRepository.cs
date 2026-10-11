using API.Models;
using Microsoft.Data.SqlClient;

namespace API.Repositories;

/// <summary>
/// ADO.NET implementation of IAuthRepository.
/// Strictly executes raw SQL queries using SqlConnection, SqlCommand, and SqlDataReader.
/// </summary>
public class AuthRepository : IAuthRepository
{
    private readonly string _connectionString;

    public AuthRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection connection string is missing.");
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = "SELECT Id, Username, Password, Role FROM Users WHERE Username = @Username";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Username", username);

        using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new User
        {
            Id = (int)reader["Id"],
            Username = reader["Username"].ToString()!,
            Password = reader["Password"].ToString()!,
            Role = reader["Role"].ToString()!
        };
    }

    public async Task<ApiClient?> GetActiveApiClientAsync(string clientId, string clientSecret)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            SELECT Id, ClientId, ClientSecret, Department, IsActive 
            FROM ApiClients 
            WHERE ClientId = @ClientId AND ClientSecret = @ClientSecret AND IsActive = 1";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ClientId", clientId);
        command.Parameters.AddWithValue("@ClientSecret", clientSecret);

        using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new ApiClient
        {
            Id = (int)reader["Id"],
            ClientId = reader["ClientId"].ToString()!,
            ClientSecret = reader["ClientSecret"].ToString()!,
            Department = reader["Department"].ToString()!,
            IsActive = (bool)reader["IsActive"]
        };
    }
}
