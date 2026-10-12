using API.DTOs;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Accessible for testing the idempotency form
public class PersonController : ControllerBase
{
    private readonly IPersonService _personService;
    private readonly ILogger<PersonController> _logger;

    public PersonController(IPersonService personService, ILogger<PersonController> logger)
    {
        _personService = personService;
        _logger = logger;
    }

    /// <summary>
    /// Submits a new Person form idempotently.
    /// Header: 'Idempotency-Key' (UUID) uniquely identifies the mutation request.
    /// If the same request is clicked multiple times, this endpoint recognizes the key
    /// and returns the original record without inserting duplicates into SQL Server.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePerson(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKeyHeader,
        [FromBody] CreatePersonDto dto)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKeyHeader))
        {
            return BadRequest(new
            {
                success = false,
                message = "Missing required 'Idempotency-Key' HTTP header. Enterprise APIs require this header to guarantee safe, non-duplicated submissions."
            });
        }

        try
        {
            var result = await _personService.CreatePersonIdempotentAsync(
                idempotencyKeyHeader.Trim(),
                dto,
                Request.Path.Value ?? "/api/person");

            // Enterprise header standard (IETF Idempotency-Key specification):
            // 'Idempotent-Replay: true' informs the client/gateway that this response was replayed.
            Response.Headers["Idempotent-Replay"] = result.IsReplay.ToString().ToLower();

            if (result.IsReplay)
            {
                // HTTP 200 OK for replayed results
                return Ok(result);
            }

            // HTTP 201 Created for initial creation
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("in-flight") || ex.Message.Contains("Concurrent"))
        {
            return Conflict(new
            {
                success = false,
                isReplay = false,
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("conflict") || ex.Message.Contains("different request"))
        {
            return StatusCode(StatusCodes.Status422UnprocessableEntity, new
            {
                success = false,
                isReplay = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing person submission.");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Retrieves all persisted records from the SQL Server 'Persons' table.
    /// Used by the UI to verify real-time DB row count.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllPersons()
    {
        var persons = await _personService.GetAllPersonsAsync();
        return Ok(persons);
    }

    /// <summary>
    /// Retrieves the IdempotencyRecords table audit log from SQL Server.
    /// </summary>
    [HttpGet("audit")]
    public async Task<IActionResult> GetIdempotencyAudit()
    {
        var logs = await _personService.GetIdempotencyAuditLogsAsync();
        return Ok(logs);
    }

    /// <summary>
    /// Utility endpoint to clear test data from Persons and IdempotencyRecords tables.
    /// </summary>
    [HttpDelete("clear-test-data")]
    public async Task<IActionResult> ClearTestData()
    {
        await _personService.ClearTestDataAsync();
        return Ok(new { message = "Persons and Idempotency audit records cleared successfully." });
    }
}
