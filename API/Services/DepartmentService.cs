using API.DTOs;

namespace API.Services;

public class DepartmentService : IDepartmentService
{
    public object GetGatewayStatus()
    {
        return new
        {
            Service = "Primary API (Port 5012)",
            Message = "Successfully reached through the YARP API Gateway!"
        };
    }

    public DepartmentAccessResultDto GetHrAccessData(string? callerId)
    {
        return new DepartmentAccessResultDto
        {
            Status = "Success",
            Message = "Access granted to Method A (Confidential HR Data).",
            Caller = callerId,
            Department = "HR"
        };
    }

    public DepartmentAccessResultDto GetFinanceAccessData(string? callerId)
    {
        return new DepartmentAccessResultDto
        {
            Status = "Success",
            Message = "Access granted to Method B (Confidential Finance Data).",
            Caller = callerId,
            Department = "Finance"
        };
    }

    public DepartmentAccessResultDto GetSharedAccessData(string? callerId, string? callerDepartment)
    {
        return new DepartmentAccessResultDto
        {
            Status = "Success",
            Message = "Access granted to Shared Common Method.",
            Caller = callerId,
            CallerDepartment = callerDepartment
        };
    }
}
