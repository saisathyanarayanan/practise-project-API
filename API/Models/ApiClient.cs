
namespace API.Models;

public class ApiClient
{
    public int Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty; // "HR" or "Finance"
    public bool IsActive { get; set; } = true;
    //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}