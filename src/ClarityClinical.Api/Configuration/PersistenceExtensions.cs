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

        return services;
    }
}
