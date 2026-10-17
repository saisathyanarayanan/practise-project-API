using API.Models;

namespace API.Repositories.Interface;

public interface IPersonRepository
{
    Task EnsureTablesExistAsync();
    Task<Person?> GetPersonByIdempotencyKeyAsync(string idempotencyKey);
    Task<int> CreatePersonAsync(Person person);
    Task<List<Person>> GetAllPersonsAsync();
    Task<IdempotencyRecord?> GetIdempotencyRecordAsync(string idempotencyKey);
    Task<bool> TryCreateIdempotencyRecordAsync(IdempotencyRecord record);
    Task UpdateIdempotencyRecordAsync(string idempotencyKey, string status, int statusCode, string responseBody);
    Task<List<IdempotencyRecord>> GetAllIdempotencyRecordsAsync();
    Task ClearTestDataAsync();
}
