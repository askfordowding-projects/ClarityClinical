using System.Net;
using ClarityClinical.Api.Tests.TestSupport;

namespace ClarityClinical.Api.Tests.Health;

public sealed class HealthEndpointTests
{
    [Fact]
    public async Task Live_health_succeeds_when_process_is_running()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();

        var response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Ready_health_succeeds_when_postgresql_is_available()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();

        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Ready_health_fails_when_postgresql_is_unavailable_but_live_stays_healthy()
    {
        await using var factory = new ClarityClinicalWebApplicationFactory(
            "Host=127.0.0.1;Port=1;Username=postgres;Database=missing;Timeout=1;Command Timeout=1");
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        var live = await client.GetAsync("/health/live");
        var ready = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, live.StatusCode);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, ready.StatusCode);
    }
}
