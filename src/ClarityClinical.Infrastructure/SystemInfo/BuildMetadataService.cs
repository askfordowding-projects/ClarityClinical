using System.Reflection;
using ClarityClinical.Application.ClinicalIntelligence;
using ClarityClinical.Application.SystemInfo;
using ClarityClinical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ClarityClinical.Infrastructure.SystemInfo;

public sealed class BuildMetadataService(
    ClarityClinicalDbContext dbContext,
    IConfiguration configuration) : IBuildMetadataService
{
    public async Task<BuildMetadataDto> GetAsync(
        string environment,
        CancellationToken cancellationToken)
    {
        var migrations = await dbContext.Database.GetAppliedMigrationsAsync(cancellationToken);
        var latestMigration = migrations.LastOrDefault() ?? "none";
        var version = configuration["Build:Version"] ?? GetAssemblyVersion();
        var commit = configuration["Build:Commit"] ?? "development";
        var timestamp = ParseTimestamp(configuration["Build:Timestamp"]);

        return new BuildMetadataDto(
            version,
            commit,
            environment,
            latestMigration,
            MiguelScenarioRuleSet.Version,
            timestamp,
            true);
    }

    private static string GetAssemblyVersion()
    {
        var version = typeof(BuildMetadataService).Assembly.GetName().Version;
        return version is null ? "development" : $"{version.Major}.{version.Minor}.{Math.Max(version.Build, 0)}";
    }

    private static DateTimeOffset? ParseTimestamp(string? value) =>
        DateTimeOffset.TryParse(value, out var timestamp) ? timestamp : null;
}
