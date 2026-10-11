using API.DTOs;
using API.Repositories;

namespace API.Services;

public class DatabaseHealthService : IDatabaseHealthService
{
    private readonly IDatabaseHealthRepository _healthRepository;

    public DatabaseHealthService(IDatabaseHealthRepository healthRepository)
    {
        _healthRepository = healthRepository;
    }

    public bool CheckEfCore()
    {
        return _healthRepository.CanConnectEfCore();
    }

    public async Task<AdoNetHealthResultDto> CheckAdoNetAsync()
    {
        var (isSuccess, dbName, state, error) = await _healthRepository.CheckAdoNetConnectionAsync();

        if (isSuccess)
        {
            return new AdoNetHealthResultDto
            {
                IsSuccess = true,
                Approach = "ADO.NET",
                Status = "Connected Successfully!",
                DatabaseName = dbName,
                ConnectionState = state
            };
        }

        return new AdoNetHealthResultDto
        {
            IsSuccess = false,
            Approach = "ADO.NET",
            Status = "Connection Failed",
            ErrorMessage = error
        };
    }
}
