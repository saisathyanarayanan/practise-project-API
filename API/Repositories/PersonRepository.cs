using API.Models;
using Microsoft.Data.SqlClient;

namespace API.Repositories;

public class PersonRepository : IPersonRepository
{
    private readonly string _connectionString;

    public PersonRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection connection string is missing.");
    }

    public async Task EnsureTablesExistAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string script = @"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Persons')
            BEGIN
                CREATE TABLE Persons (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    IdempotencyKey NVARCHAR(100) NOT NULL,
                    Name NVARCHAR(100) NOT NULL,
                    Age INT NOT NULL,
                    Sex NVARCHAR(20) NOT NULL,
                    PhoneNumber NVARCHAR(30) NOT NULL,
                    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
                );
                CREATE UNIQUE INDEX UX_Persons_IdempotencyKey ON Persons(IdempotencyKey);
            END;

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IdempotencyRecords')
            BEGIN
                CREATE TABLE IdempotencyRecords (
                    IdempotencyKey NVARCHAR(100) PRIMARY KEY,
                    RequestPath NVARCHAR(200) NOT NULL,
                    RequestHash NVARCHAR(128) NOT NULL,
                    Status NVARCHAR(50) NOT NULL,
                    StatusCode INT NULL,
                    ResponseBody NVARCHAR(MAX) NULL,
                    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                    ExpiresAt DATETIME2 NULL
                );
            END;
        ";

        using var command = new SqlCommand(script, connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<Person?> GetPersonByIdempotencyKeyAsync(string idempotencyKey)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            SELECT TOP 1 Id, IdempotencyKey, Name, Age, Sex, PhoneNumber, CreatedAt
            FROM Persons
            WHERE IdempotencyKey = @IdempotencyKey;";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@IdempotencyKey", idempotencyKey);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Person
            {
                Id = Convert.ToInt32(reader["Id"]),
                IdempotencyKey = reader["IdempotencyKey"].ToString()!,
                Name = reader["Name"].ToString()!,
                Age = Convert.ToInt32(reader["Age"]),
                Sex = reader["Sex"].ToString()!,
                PhoneNumber = reader["PhoneNumber"].ToString()!,
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            };
        }

        return null;
    }

    public async Task<int> CreatePersonAsync(Person person)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            INSERT INTO Persons (IdempotencyKey, Name, Age, Sex, PhoneNumber, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (@IdempotencyKey, @Name, @Age, @Sex, @PhoneNumber, SYSUTCDATETIME());";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@IdempotencyKey", person.IdempotencyKey);
        command.Parameters.AddWithValue("@Name", person.Name);
        command.Parameters.AddWithValue("@Age", person.Age);
        command.Parameters.AddWithValue("@Sex", person.Sex);
        command.Parameters.AddWithValue("@PhoneNumber", person.PhoneNumber);

        var insertedId = await command.ExecuteScalarAsync();
        return Convert.ToInt32(insertedId);
    }

    public async Task<List<Person>> GetAllPersonsAsync()
    {
        var list = new List<Person>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            SELECT Id, IdempotencyKey, Name, Age, Sex, PhoneNumber, CreatedAt
            FROM Persons
            ORDER BY Id DESC;";

        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new Person
            {
                Id = Convert.ToInt32(reader["Id"]),
                IdempotencyKey = reader["IdempotencyKey"].ToString()!,
                Name = reader["Name"].ToString()!,
                Age = Convert.ToInt32(reader["Age"]),
                Sex = reader["Sex"].ToString()!,
                PhoneNumber = reader["PhoneNumber"].ToString()!,
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            });
        }

        return list;
    }

    public async Task<IdempotencyRecord?> GetIdempotencyRecordAsync(string idempotencyKey)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            SELECT TOP 1 IdempotencyKey, RequestPath, RequestHash, Status, StatusCode, ResponseBody, CreatedAt, ExpiresAt
            FROM IdempotencyRecords
            WHERE IdempotencyKey = @IdempotencyKey;";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@IdempotencyKey", idempotencyKey);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new IdempotencyRecord
            {
                IdempotencyKey = reader["IdempotencyKey"].ToString()!,
                RequestPath = reader["RequestPath"].ToString()!,
                RequestHash = reader["RequestHash"].ToString()!,
                Status = reader["Status"].ToString()!,
                StatusCode = reader["StatusCode"] != DBNull.Value ? Convert.ToInt32(reader["StatusCode"]) : null,
                ResponseBody = reader["ResponseBody"] != DBNull.Value ? reader["ResponseBody"].ToString() : null,
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                ExpiresAt = reader["ExpiresAt"] != DBNull.Value ? Convert.ToDateTime(reader["ExpiresAt"]) : null
            };
        }

        return null;
    }

    public async Task<bool> TryCreateIdempotencyRecordAsync(IdempotencyRecord record)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // Atomically insert only if key doesn't already exist
        const string query = @"
            BEGIN TRY
                INSERT INTO IdempotencyRecords (IdempotencyKey, RequestPath, RequestHash, Status, CreatedAt, ExpiresAt)
                VALUES (@IdempotencyKey, @RequestPath, @RequestHash, @Status, SYSUTCDATETIME(), @ExpiresAt);
                SELECT 1;
            END TRY
            BEGIN CATCH
                SELECT 0;
            END CATCH;";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@IdempotencyKey", record.IdempotencyKey);
        command.Parameters.AddWithValue("@RequestPath", record.RequestPath);
        command.Parameters.AddWithValue("@RequestHash", record.RequestHash);
        command.Parameters.AddWithValue("@Status", record.Status);
        command.Parameters.AddWithValue("@ExpiresAt", (object?)record.ExpiresAt ?? DBNull.Value);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) == 1;
    }

    public async Task UpdateIdempotencyRecordAsync(string idempotencyKey, string status, int statusCode, string responseBody)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            UPDATE IdempotencyRecords
            SET Status = @Status,
                StatusCode = @StatusCode,
                ResponseBody = @ResponseBody
            WHERE IdempotencyKey = @IdempotencyKey;";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Status", status);
        command.Parameters.AddWithValue("@StatusCode", statusCode);
        command.Parameters.AddWithValue("@ResponseBody", responseBody ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@IdempotencyKey", idempotencyKey);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<IdempotencyRecord>> GetAllIdempotencyRecordsAsync()
    {
        var list = new List<IdempotencyRecord>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            SELECT TOP 50 IdempotencyKey, RequestPath, RequestHash, Status, StatusCode, ResponseBody, CreatedAt, ExpiresAt
            FROM IdempotencyRecords
            ORDER BY CreatedAt DESC;";

        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new IdempotencyRecord
            {
                IdempotencyKey = reader["IdempotencyKey"].ToString()!,
                RequestPath = reader["RequestPath"].ToString()!,
                RequestHash = reader["RequestHash"].ToString()!,
                Status = reader["Status"].ToString()!,
                StatusCode = reader["StatusCode"] != DBNull.Value ? Convert.ToInt32(reader["StatusCode"]) : null,
                ResponseBody = reader["ResponseBody"] != DBNull.Value ? reader["ResponseBody"].ToString() : null,
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                ExpiresAt = reader["ExpiresAt"] != DBNull.Value ? Convert.ToDateTime(reader["ExpiresAt"]) : null
            });
        }

        return list;
    }

    public async Task ClearTestDataAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            DELETE FROM Persons;
            DELETE FROM IdempotencyRecords;";

        using var command = new SqlCommand(query, connection);
        await command.ExecuteNonQueryAsync();
    }
}
