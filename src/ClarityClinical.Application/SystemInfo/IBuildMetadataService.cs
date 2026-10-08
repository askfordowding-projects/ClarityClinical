namespace ClarityClinical.Application.SystemInfo;

public sealed record BuildMetadataDto(
    string Version,
    string Commit,
    string Environment,
    string LatestMigration,
    string RuleSetVersion,
    DateTimeOffset? BuildTimestamp,
    bool IsDemo);

public interface IBuildMetadataService
{
    Task<BuildMetadataDto> GetAsync(string environment, CancellationToken cancellationToken);
}
