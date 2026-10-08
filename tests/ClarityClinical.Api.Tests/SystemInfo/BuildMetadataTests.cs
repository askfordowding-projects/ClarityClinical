using System.Net;
using System.Net.Http.Json;
using ClarityClinical.Api.Tests.TestSupport;

namespace ClarityClinical.Api.Tests.SystemInfo;

public sealed class BuildMetadataTests
{
    [Fact]
    public async Task Build_metadata_is_public_and_identifies_runtime_database_and_rule_set()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();

        var response = await client.GetAsync("/api/system/build");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var metadata = await response.Content.ReadFromJsonAsync<BuildMetadataResponse>();
        Assert.NotNull(metadata);
        Assert.Equal("Testing", metadata!.Environment);
        Assert.False(string.IsNullOrWhiteSpace(metadata.Version));
        Assert.False(string.IsNullOrWhiteSpace(metadata.Commit));
        Assert.StartsWith("2026", metadata.LatestMigration, StringComparison.Ordinal);
        Assert.Equal("demo-illustrative-1.0", metadata.RuleSetVersion);
        Assert.True(metadata.IsDemo);
    }

    private sealed record BuildMetadataResponse(
        string Version,
        string Commit,
        string Environment,
        string LatestMigration,
        string RuleSetVersion,
        DateTimeOffset? BuildTimestamp,
        bool IsDemo);
}
