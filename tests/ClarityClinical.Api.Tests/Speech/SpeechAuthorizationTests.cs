using System.Net;
using System.Net.Http.Json;
using ClarityClinical.Api.Tests.TestSupport;
using ClarityClinical.Application.Speech;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ClarityClinical.Api.Tests.Speech;

public sealed class SpeechAuthorizationTests
{
    [Fact]
    public async Task Unauthenticated_request_is_rejected()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();

        var response = await client.PostAsync("/api/speech/authorization", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Authenticated_request_returns_service_unavailable_when_speech_is_not_configured()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        (await client.PostAsJsonAsync("/api/auth/demo-login", new { role = "Clinician" }))
            .EnsureSuccessStatusCode();

        var response = await client.PostAsync("/api/speech/authorization", null);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Configured_provider_returns_short_lived_token_contract()
    {
        var refreshAfter = DateTimeOffset.UtcNow.AddMinutes(9);
        await using var context = await ApiTestContext.CreateAsync(services =>
        {
            services.RemoveAll<ISpeechAuthorizationProvider>();
            services.AddSingleton<ISpeechAuthorizationProvider>(
                new FakeSpeechAuthorizationProvider(
                    new SpeechAuthorization("short-lived-token", "uksouth", refreshAfter)));
        });
        using var client = context.CreateClient();
        (await client.PostAsJsonAsync("/api/auth/demo-login", new { role = "Clinician" }))
            .EnsureSuccessStatusCode();

        var response = await client.PostAsync("/api/speech/authorization", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<SpeechAuthorization>();
        Assert.Equal("short-lived-token", body?.Token);
        Assert.Equal("uksouth", body?.Region);
        Assert.Equal(refreshAfter, body?.RefreshAfter);
        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("subscription", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("key", json, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FakeSpeechAuthorizationProvider(SpeechAuthorization authorization)
        : ISpeechAuthorizationProvider
    {
        public Task<SpeechAuthorization> GetAuthorizationAsync(CancellationToken cancellationToken) =>
            Task.FromResult(authorization);
    }}
