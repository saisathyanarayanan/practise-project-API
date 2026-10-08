namespace API.Cors;

public class CorsSettings
{
    public const string SectionName = "Cors";

    public string PolicyName { get; set; } = "UiCors";
    public string[] AllowedOrigins { get; set; } = [];
    public string[] AllowedMethods { get; set; } = [];
    public string[] AllowedHeaders { get; set; } = [];
    public bool AllowCredentials { get; set; }
}
