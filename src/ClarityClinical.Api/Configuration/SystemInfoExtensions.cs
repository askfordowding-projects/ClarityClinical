using ClarityClinical.Application.SystemInfo;
using ClarityClinical.Infrastructure.SystemInfo;

namespace ClarityClinical.Api.Configuration;

public static class SystemInfoExtensions
{
    public static IServiceCollection AddClarityClinicalSystemInfo(this IServiceCollection services)
    {
        services.AddScoped<IBuildMetadataService, BuildMetadataService>();
        return services;
    }
}
