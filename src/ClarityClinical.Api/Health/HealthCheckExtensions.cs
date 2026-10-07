using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace ClarityClinical.Api.Health;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddClarityClinicalHealthChecks(
        this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<PostgreSqlReadinessHealthCheck>(
                "postgresql",
                tags: ["ready"]);
        return services;
    }

    public static WebApplication MapClarityClinicalHealthChecks(
        this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready")
        });
        return app;
    }
}
