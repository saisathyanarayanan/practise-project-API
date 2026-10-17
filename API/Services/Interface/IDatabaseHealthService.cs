using API.DTOs;

namespace API.Services.Interface;

public interface IDatabaseHealthService
{
    bool CheckEfCore();
    Task<AdoNetHealthResultDto> CheckAdoNetAsync();
}
