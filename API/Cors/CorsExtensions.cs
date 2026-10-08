namespace API.Cors;

public static class CorsExtensions
{
    public static IServiceCollection AddAppCors(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>()
            ?? throw new InvalidOperationException("Cors section is missing from CorsSettings.json.");

        if (settings.AllowedOrigins.Length == 0)
        {
            throw new InvalidOperationException("Cors:AllowedOrigins must contain at least one origin.");
        }

        services.AddCors(options =>
        {
            options.AddPolicy(settings.PolicyName, policy =>
            {
                policy.WithOrigins(settings.AllowedOrigins)
                    .WithMethods(settings.AllowedMethods)
                    .WithHeaders(settings.AllowedHeaders);

                if (settings.AllowCredentials)
                {
                    policy.AllowCredentials();
                }
            });
        });

        services.AddSingleton(settings);
        return services;
    }

    public static IApplicationBuilder UseAppCors(this IApplicationBuilder app, IConfiguration configuration)
    {
        var settings = configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>()
            ?? throw new InvalidOperationException("Cors section is missing from CorsSettings.json.");

        app.UseCors(settings.PolicyName);
        return app;
    }
}
