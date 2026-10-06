using System.Net;
using System.Net.Http.Json;
using ClarityClinical.Api.Tests.TestSupport;

namespace ClarityClinical.Api.Tests.Demo;

public sealed class PublicDemoSessionAccessTests
{
    [Fact]
    public async Task Demo_visitors_cannot_access_or_mutate_each_others_consultations()
    {
        await using var context = await ApiTestContext.CreateAsync();
        await context.SeedMiguelScenarioAsync();
        using var visitorA = context.CreateClient();
        using var visitorB = context.CreateClient();
        await LoginAsClinicianAsync(visitorA);
        await LoginAsClinicianAsync(visitorB);

        var sessionA = await CreateSessionAsync(visitorA);
        var sessionB = await CreateSessionAsync(visitorB);
        Assert.NotEqual(sessionA.ConsultationId, sessionB.ConsultationId);

        Assert.Equal(HttpStatusCode.OK, (await visitorA.PostAsync(
            $"/api/consultations/{sessionA.ConsultationId}/start", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await visitorB.GetAsync(
            $"/api/consultations/{sessionA.ConsultationId}/workspace")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await visitorB.PostAsJsonAsync(
            $"/api/consultations/{sessionA.ConsultationId}/observations",
            new { code = "calf-asymmetry", displayText = "Calf asymmetry" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await visitorB.PostAsync(
            $"/api/consultations/{sessionA.ConsultationId}/complete", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await visitorB.PostAsync(
            $"/api/demo/sessions/{sessionA.Id}/reset", null)).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await visitorA.GetAsync(
            $"/api/consultations/{sessionA.ConsultationId}/workspace")).StatusCode);
    }

    private static async Task LoginAsClinicianAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/demo-login", new { role = "Clinician" });
        response.EnsureSuccessStatusCode();
    }

    private static async Task<DemoSessionResponse> CreateSessionAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/demo/sessions",
            new { scenarioKey = "miguel-santos-leg-swelling" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<DemoSessionResponse>())!;
    }

    private sealed record DemoSessionResponse(Guid Id, Guid ConsultationId);
}
