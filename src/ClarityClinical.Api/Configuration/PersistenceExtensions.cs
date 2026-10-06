using ClarityClinical.Application.Demo;
using ClarityClinical.Infrastructure.Demo;
using ClarityClinical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClarityClinical.Api.Configuration;

public static class PersistenceExtensions
{
    public static IServiceCollection AddClarityClinicalPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ClarityClinical")
            ?? throw new InvalidOperationException(
                "Connection string 'ClarityClinical' is not configured.");

        services.AddDbContext<ClarityClinicalDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<IDemoScenarioRepository, DemoScenarioRepository>();
        services.AddScoped<DemoScenarioSeeder>();

        return services;
    }
}
