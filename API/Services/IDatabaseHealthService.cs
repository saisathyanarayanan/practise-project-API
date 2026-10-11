using API.DTOs;

namespace API.Services;

public interface IDatabaseHealthService
{
    bool CheckEfCore();
    Task<AdoNetHealthResultDto> CheckAdoNetAsync();
}
