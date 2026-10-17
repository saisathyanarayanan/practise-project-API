using API.Models;
using API.Repositories.Interface;
using Microsoft.Data.SqlClient;

namespace API.Repositories.Implementation;

public class UserManagementRepository : IUserManagementRepository
{
    private readonly string _connectionString;

    public UserManagementRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection connection string is missing.");
    }

    public async Task<int> CreateHashedUserAsync(HashedUser user)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            INSERT INTO maskedpasssword_usersTable (Username, PasswordHash, Salt, Role, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (@Username, @PasswordHash, @Salt, @Role, SYSUTCDATETIME());";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Username", user.Username);
        command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        command.Parameters.AddWithValue("@Salt", user.Salt);
        command.Parameters.AddWithValue("@Role", user.Role);

        var insertedId = await command.ExecuteScalarAsync();
        return Convert.ToInt32(insertedId);
    }

    public async Task<List<HashedUser>> GetHashedUsersAsync()
    {
        var list = new List<HashedUser>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            SELECT Id, Username, PasswordHash, Salt, Role, CreatedAt 
            FROM maskedpasssword_usersTable 
            ORDER BY Id DESC;";

        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new HashedUser
            {
                Id = (int)reader["Id"],
                Username = reader["Username"].ToString()!,
                PasswordHash = reader["PasswordHash"].ToString()!,
                Salt = reader["Salt"].ToString()!,
                Role = reader["Role"].ToString()!,
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            });
        }

        return list;
    }

    public async Task<HashedUser?> GetHashedUserByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            SELECT Id, Username, PasswordHash, Salt, Role, CreatedAt 
            FROM maskedpasssword_usersTable 
            WHERE Id = @Id;";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new HashedUser
        {
            Id = (int)reader["Id"],
            Username = reader["Username"].ToString()!,
            PasswordHash = reader["PasswordHash"].ToString()!,
            Salt = reader["Salt"].ToString()!,
            Role = reader["Role"].ToString()!,
            CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
        };
    }

    public async Task<int> CreateDirectUserAsync(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            INSERT INTO Users (Username, Password, Role)
            OUTPUT INSERTED.Id
            VALUES (@Username, @Password, @Role);";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Username", user.Username);
        command.Parameters.AddWithValue("@Password", user.Password);
        command.Parameters.AddWithValue("@Role", user.Role);

        var insertedId = await command.ExecuteScalarAsync();
        return Convert.ToInt32(insertedId);
    }

    public async Task<List<User>> GetUsersAsync(bool asMaskedViewer = false)
    {
        var list = new List<User>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // If asMaskedViewer is requested, simulate a non-privileged user to demonstrate SQL Server Dynamic Data Masking (DDM)
        string query = asMaskedViewer
            ? @"
                EXECUTE AS USER = 'MaskedDataUser';
                SELECT Id, Username, Password, Role FROM Users ORDER BY Id DESC;
                REVERT;"
            : @"
                SELECT Id, Username, Password, Role FROM Users ORDER BY Id DESC;";

        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new User
            {
                Id = (int)reader["Id"],
                Username = reader["Username"].ToString()!,
                Password = reader["Password"].ToString()!,
                Role = reader["Role"].ToString()!
            });
        }

        return list;
    }
}
