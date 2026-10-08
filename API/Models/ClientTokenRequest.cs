namespace API.Models;

public class ClientTokenRequest
{
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
}