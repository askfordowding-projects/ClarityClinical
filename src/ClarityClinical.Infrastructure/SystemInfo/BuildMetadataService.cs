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
        var runtimeReleaseTag = configuration["AFD_RELEASE_TAG"];
        var version = !string.IsNullOrWhiteSpace(runtimeReleaseTag)
            ? runtimeReleaseTag.Trim()
            : configuration["Build:Version"] ?? GetAssemblyVersion();
        var runtimeCommit = configuration["AFD_GIT_SHA"];
        var commit = !string.IsNullOrWhiteSpace(runtimeCommit)
            ? runtimeCommit.Trim()
            : configuration["Build:Commit"] ?? "development";
        var timestamp = ParseTimestamp(configuration["Build:Timestamp"]);
        // Returned verbatim: the deployment platform verifies the exact value it wrote.
        var releasedAtUtc = VerbatimTimestamp(configuration["AFD_RELEASED_AT_UTC"]);

        return new BuildMetadataDto(
            version,
            commit,
            environment,
            latestMigration,
            MiguelScenarioRuleSet.Version,
            timestamp,
            releasedAtUtc,
            true);
    }

    private static string GetAssemblyVersion()
    {
        var version = typeof(BuildMetadataService).Assembly.GetName().Version;
        return version is null ? "development" : $"{version.Major}.{version.Minor}.{Math.Max(version.Build, 0)}";
    }

    private static string? VerbatimTimestamp(string? value) =>
        DateTimeOffset.TryParse(value, out _) ? value!.Trim() : null;

    private static DateTimeOffset? ParseTimestamp(string? value) =>
        DateTimeOffset.TryParse(value, out var timestamp) ? timestamp.ToUniversalTime() : null;
}
