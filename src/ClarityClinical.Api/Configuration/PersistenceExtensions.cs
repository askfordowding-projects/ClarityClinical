using ClarityClinical.Application.Consultations;
using ClarityClinical.Application.Demo;
using ClarityClinical.Application.Identity;
using ClarityClinical.Infrastructure.Consultations;
using ClarityClinical.Infrastructure.Demo;
using ClarityClinical.Infrastructure.Identity;
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
        services.AddScoped<IDemoIdentityProvider, DemoIdentityProvider>();
        services.AddScoped<IConsultationRepository, ConsultationRepository>();
        services.AddScoped<ConsultationLifecycleService>();

        return services;
    }
}
