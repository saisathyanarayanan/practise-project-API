using API.DTOs;

namespace API.Services.Interface;

public interface IDepartmentService
{
    object GetGatewayStatus();
    DepartmentAccessResultDto GetHrAccessData(string? callerId);
    DepartmentAccessResultDto GetFinanceAccessData(string? callerId);
    DepartmentAccessResultDto GetSharedAccessData(string? callerId, string? callerDepartment);
}
