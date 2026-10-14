using System.Net;
using System.Net.Http.Json;
using ClarityClinical.Api.Tests.TestSupport;

namespace ClarityClinical.Api.Tests.SystemInfo;

public sealed class BuildMetadataTests
{
    [Fact]
    public async Task Build_metadata_is_public_and_reports_platform_release_identity()
    {
        var oldTag = Environment.GetEnvironmentVariable("AFD_RELEASE_TAG");
        var oldSha = Environment.GetEnvironmentVariable("AFD_GIT_SHA");
        var oldReleased = Environment.GetEnvironmentVariable("AFD_RELEASED_AT_UTC");
        try
        {
            Environment.SetEnvironmentVariable("AFD_RELEASE_TAG", "v6.7.8.9");
            Environment.SetEnvironmentVariable("AFD_GIT_SHA", "0123456789abcdef0123456789abcdef01234567");
            Environment.SetEnvironmentVariable("AFD_RELEASED_AT_UTC", "2026-10-14T15:25:00Z");
            await using var context = await ApiTestContext.CreateAsync();
            using var client = context.CreateClient();

            var response = await client.GetAsync("/api/system/build");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var metadata = await response.Content.ReadFromJsonAsync<BuildMetadataResponse>();
            Assert.NotNull(metadata);
            Assert.Equal("Testing", metadata!.Environment);
            Assert.Equal("v6.7.8.9", metadata.Version);
            Assert.Equal("0123456789abcdef0123456789abcdef01234567", metadata.Commit);
            Assert.Equal(new DateTimeOffset(2026, 10, 14, 15, 25, 0, TimeSpan.Zero), metadata.ReleasedAtUtc);
            Assert.StartsWith("2026", metadata.LatestMigration, StringComparison.Ordinal);
            Assert.Equal("demo-illustrative-1.0", metadata.RuleSetVersion);
            Assert.True(metadata.IsDemo);
        }
        finally
        {
            Environment.SetEnvironmentVariable("AFD_RELEASE_TAG", oldTag);
            Environment.SetEnvironmentVariable("AFD_GIT_SHA", oldSha);
            Environment.SetEnvironmentVariable("AFD_RELEASED_AT_UTC", oldReleased);
        }
    }

    private sealed record BuildMetadataResponse(
        string Version,
        string Commit,
        string Environment,
        string LatestMigration,
        string RuleSetVersion,
        DateTimeOffset? BuildTimestamp,
        DateTimeOffset? ReleasedAtUtc,
        bool IsDemo);
}
