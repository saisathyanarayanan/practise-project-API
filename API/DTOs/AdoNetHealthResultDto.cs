namespace API.DTOs;

public class AdoNetHealthResultDto
{
    public bool IsSuccess { get; set; }
    public string Approach { get; set; } = "ADO.NET";
    public string Status { get; set; } = string.Empty;
    public string? DatabaseName { get; set; }
    public string? ConnectionState { get; set; }
    public string? ErrorMessage { get; set; }
}
