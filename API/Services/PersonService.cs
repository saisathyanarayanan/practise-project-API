using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.DTOs;
using API.Models;
using API.Repositories;

namespace API.Services;

public class PersonService : IPersonService
{
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<PersonService> _logger;

    public PersonService(IPersonRepository personRepository, ILogger<PersonService> logger)
    {
        _personRepository = personRepository;
        _logger = logger;
    }

    public async Task<IdempotentApiResponse<PersonResponseDto>> CreatePersonIdempotentAsync(
        string idempotencyKey,
        CreatePersonDto dto,
        string requestPath)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("Idempotency-Key header is required to safely process this mutation.");
        }

        // 1. Compute deterministic hash of the request payload (SHA-256)
        var payloadJson = JsonSerializer.Serialize(dto);
        var currentPayloadHash = ComputeSha256(payloadJson);

        // 2. Check if this idempotency key was already recorded
        var existingRecord = await _personRepository.GetIdempotencyRecordAsync(idempotencyKey);

        if (existingRecord != null)
        {
            // Case A: Duplicate request arrives while the original is still processing
            if (existingRecord.Status == "Processing")
            {
                _logger.LogWarning("Concurrent request detected for Idempotency-Key: {Key}", idempotencyKey);
                throw new InvalidOperationException("A request with this Idempotency-Key is currently in-flight. Please wait.");
            }

            // Case B: Request already completed, but payload does not match original
            if (!string.Equals(existingRecord.RequestHash, currentPayloadHash, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Idempotency key {Key} payload mismatch detected.", idempotencyKey);
                throw new InvalidOperationException("Idempotency-Key conflict: this key has already been used with different request parameters.");
            }

            // Case C: Standard Idempotent Replay!
            // Return cached response without modifying or duplicating DB records.
            _logger.LogInformation("Idempotent replay served for key: {Key}", idempotencyKey);

            if (!string.IsNullOrEmpty(existingRecord.ResponseBody))
            {
                var cachedPerson = JsonSerializer.Deserialize<PersonResponseDto>(existingRecord.ResponseBody);
                return new IdempotentApiResponse<PersonResponseDto>
                {
                    Success = true,
                    IsReplay = true,
                    IdempotencyKey = idempotencyKey,
                    Message = "IDEMPOTENT REPLAY: Request was previously processed. Returned existing record safely without duplicating in DB.",
                    Data = cachedPerson,
                    ProcessedAt = existingRecord.CreatedAt
                };
            }

            // Fallback: look up directly from Persons table
            var existingPerson = await _personRepository.GetPersonByIdempotencyKeyAsync(idempotencyKey);
            return new IdempotentApiResponse<PersonResponseDto>
            {
                Success = true,
                IsReplay = true,
                IdempotencyKey = idempotencyKey,
                Message = "IDEMPOTENT REPLAY: Record already exists in DB for this Idempotency-Key. No duplicate inserted.",
                Data = existingPerson != null ? MapToDto(existingPerson) : null,
                ProcessedAt = existingRecord.CreatedAt
            };
        }

        // 3. First-time request with this key: Register as "Processing"
        var newRecord = new IdempotencyRecord
        {
            IdempotencyKey = idempotencyKey,
            RequestPath = requestPath,
            RequestHash = currentPayloadHash,
            Status = "Processing",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24) // 24hr TTL standard
        };

        var insertedLock = await _personRepository.TryCreateIdempotencyRecordAsync(newRecord);
        if (!insertedLock)
        {
            // Race condition: another thread inserted between our check and insert
            var concurrentRecord = await _personRepository.GetIdempotencyRecordAsync(idempotencyKey);
            if (concurrentRecord != null && concurrentRecord.Status == "Completed" && !string.IsNullOrEmpty(concurrentRecord.ResponseBody))
            {
                var cached = JsonSerializer.Deserialize<PersonResponseDto>(concurrentRecord.ResponseBody);
                return new IdempotentApiResponse<PersonResponseDto>
                {
                    Success = true,
                    IsReplay = true,
                    IdempotencyKey = idempotencyKey,
                    Message = "IDEMPOTENT REPLAY (Race Resolved): Returned cached record safely without duplicates.",
                    Data = cached,
                    ProcessedAt = concurrentRecord.CreatedAt
                };
            }

            throw new InvalidOperationException("Concurrent request for this Idempotency-Key detected. Please retry.");
        }

        try
        {
            // 4. Execute the business logic: Insert person into database
            var person = new Person
            {
                IdempotencyKey = idempotencyKey,
                Name = dto.Name.Trim(),
                Age = dto.Age,
                Sex = dto.Sex.Trim(),
                PhoneNumber = dto.PhoneNumber.Trim()
            };

            var newId = await _personRepository.CreatePersonAsync(person);
            person.Id = newId;
            person.CreatedAt = DateTime.UtcNow;

            var personDto = MapToDto(person);
            var serializedResponse = JsonSerializer.Serialize(personDto);

            // 5. Update Idempotency record status to "Completed"
            await _personRepository.UpdateIdempotencyRecordAsync(idempotencyKey, "Completed", 201, serializedResponse);

            _logger.LogInformation("New record created with Idempotency-Key: {Key}, PersonId: {Id}", idempotencyKey, newId);

            return new IdempotentApiResponse<PersonResponseDto>
            {
                Success = true,
                IsReplay = false,
                IdempotencyKey = idempotencyKey,
                Message = "SUCCESS: Record created and persisted in DB for the first time.",
                Data = personDto,
                ProcessedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create person for Idempotency-Key: {Key}", idempotencyKey);
            await _personRepository.UpdateIdempotencyRecordAsync(idempotencyKey, "Failed", 500, ex.Message);
            throw;
        }
    }

    public async Task<List<PersonResponseDto>> GetAllPersonsAsync()
    {
        var persons = await _personRepository.GetAllPersonsAsync();
        return persons.Select(MapToDto).ToList();
    }

    public async Task<List<IdempotencyRecord>> GetIdempotencyAuditLogsAsync()
    {
        return await _personRepository.GetAllIdempotencyRecordsAsync();
    }

    public async Task ClearTestDataAsync()
    {
        await _personRepository.ClearTestDataAsync();
    }

    private static PersonResponseDto MapToDto(Person p) => new()
    {
        Id = p.Id,
        IdempotencyKey = p.IdempotencyKey,
        Name = p.Name,
        Age = p.Age,
        Sex = p.Sex,
        PhoneNumber = p.PhoneNumber,
        CreatedAt = p.CreatedAt
    };

    private static string ComputeSha256(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        var sb = new StringBuilder();
        foreach (var b in bytes)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }
}
