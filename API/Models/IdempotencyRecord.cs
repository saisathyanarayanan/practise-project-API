namespace API.Models;

public class IdempotencyRecord
{
    public string IdempotencyKey { get; set; } = string.Empty;
    public string RequestPath { get; set; } = string.Empty;
    public string RequestHash { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "Processing", "Completed", "Failed"
    public int? StatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
