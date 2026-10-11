namespace API.Repositories;

/// <summary>
/// Abstraction for database connectivity and health diagnostics.
/// Preserves both EF Core and ADO.NET checks.
/// </summary>
public interface IDatabaseHealthRepository
{
    bool CanConnectEfCore();
    Task<(bool isSuccess, string? databaseName, string? connectionState, string? errorMessage)> CheckAdoNetConnectionAsync();
}
