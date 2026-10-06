using System.Net;
using System.Net.Http.Json;
using ClarityClinical.Api.Tests.TestSupport;

namespace ClarityClinical.Api.Tests.Auth;

public sealed class DemoLoginTests
{
    [Fact]
    public async Task Clinician_demo_login_creates_authenticated_session()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();

        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/demo-login",
            new { role = "Clinician" });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var meResponse = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var me = await meResponse.Content.ReadFromJsonAsync<CurrentUserResponse>();
        Assert.NotNull(me);
        Assert.Equal("Demo Clinician", me.DisplayName);
        Assert.Equal("Clinician", me.Role);
        Assert.True(me.IsDemo);
    }

    [Fact]
    public async Task Unknown_demo_role_is_rejected()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/demo-login",
            new { role = "SuperUser" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private sealed record CurrentUserResponse(string DisplayName, string Role, bool IsDemo);
}
