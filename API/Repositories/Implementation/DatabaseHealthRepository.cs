using API.Data;
using API.Repositories.Interface;
using Microsoft.Data.SqlClient;

namespace API.Repositories.Implementation;

/// <summary>
/// Implementation of IDatabaseHealthRepository.
/// Preserves EF Core CanConnect() and ADO.NET raw SQL connection checks.
/// </summary>
public class DatabaseHealthRepository : IDatabaseHealthRepository
{
    private readonly AppDbContext _context;
    private readonly string _connectionString;

    public DatabaseHealthRepository(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection connection string is missing.");
    }

    // 1. EF Core Check
    public bool CanConnectEfCore()
    {
        return _context.Database.CanConnect();
    }

    // 2. ADO.NET Check
    public async Task<(bool isSuccess, string? databaseName, string? connectionState, string? errorMessage)> CheckAdoNetConnectionAsync()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            const string query = "SELECT DB_NAME() AS CurrentDatabase";
            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                string dbName = reader["CurrentDatabase"].ToString()!;
                return (true, dbName, connection.State.ToString(), null);
            }

            return (true, null, connection.State.ToString(), null);
        }
        catch (Exception ex)
        {
            return (false, null, null, ex.Message);
        }
    }
}
