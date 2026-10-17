using API.DTOs;
using API.Models;

namespace API.Services.Interface;

public interface IPersonService
{
    Task<IdempotentApiResponse<PersonResponseDto>> CreatePersonIdempotentAsync(string idempotencyKey, CreatePersonDto dto, string requestPath);
    Task<List<PersonResponseDto>> GetAllPersonsAsync();
    Task<List<IdempotencyRecord>> GetIdempotencyAuditLogsAsync();
    Task ClearTestDataAsync();
}
