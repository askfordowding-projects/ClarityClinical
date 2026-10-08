using System.Net;
using System.Net.Http.Json;
using ClarityClinical.Api.Tests.TestSupport;

namespace ClarityClinical.Api.Tests.Admin;

public sealed class DemoAdministrationTests
{
    [Fact]
    public async Task Administrator_can_list_canonical_synthetic_patients()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        await LoginAsync(client);
        await context.SeedMiguelScenarioAsync();

        var response = await client.GetAsync("/api/admin/demo/patients");

        response.EnsureSuccessStatusCode();
        var patients = await response.Content.ReadFromJsonAsync<IReadOnlyList<PatientResponse>>();
        var miguel = Assert.Single(patients!);
        Assert.Equal("Miguel Santos", miguel.DisplayName);
        Assert.Equal(3, miguel.Conditions.Count);
        Assert.Equal(2, miguel.Allergies.Count);
        Assert.Equal(4, miguel.Medications.Count);
        Assert.True(miguel.IsCanonical);
    }

    [Fact]
    public async Task Administrator_can_list_canonical_demo_scenarios()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        await LoginAsync(client);
        await context.SeedMiguelScenarioAsync();

        var response = await client.GetAsync("/api/admin/demo/scenarios");

        response.EnsureSuccessStatusCode();
        var scenarios = await response.Content.ReadFromJsonAsync<IReadOnlyList<ScenarioResponse>>();
        var miguel = Assert.Single(scenarios!);
        Assert.Equal("miguel-santos-leg-swelling", miguel.ScenarioKey);
        Assert.Equal("Urgent Treatment Centre", miguel.Setting);
        Assert.True(miguel.IsCanonical);
        Assert.Equal(4, miguel.StepCount);
    }

    [Fact]
    public async Task Administrator_can_reset_only_their_own_demo_session()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var adminA = context.CreateClient();
        using var adminB = context.CreateClient();
        await LoginAsync(adminA);
        await LoginAsync(adminB);
        await context.SeedMiguelScenarioAsync();
        var a = await CreateSessionAsync(adminA);
        var b = await CreateSessionAsync(adminB);

        var response = await adminA.PostAsync($"/api/admin/demo/sessions/{a.Id}/reset", null);
        response.EnsureSuccessStatusCode();
        var reset = await response.Content.ReadFromJsonAsync<SessionResponse>();
        Assert.NotEqual(a.ConsultationId, reset!.ConsultationId);

        var forbidden = await adminA.PostAsync($"/api/admin/demo/sessions/{b.Id}/reset", null);
        Assert.Equal(HttpStatusCode.NotFound, forbidden.StatusCode);
    }

    [Fact]
    public async Task Clinician_cannot_use_admin_demo_catalogue()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        (await client.PostAsJsonAsync("/api/auth/demo-login", new { role = "Clinician" })).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/admin/demo/patients")).StatusCode);
    }

    private static async Task LoginAsync(HttpClient client) =>
        (await client.PostAsJsonAsync("/api/auth/demo-login", new { role = "Administrator" })).EnsureSuccessStatusCode();

    private static async Task<SessionResponse> CreateSessionAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/demo/sessions", new { scenarioKey = "miguel-santos-leg-swelling" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SessionResponse>())!;
    }

    private sealed record PatientResponse(Guid Id, string DisplayName, DateOnly DateOfBirth, bool IsCanonical, IReadOnlyList<ValueResponse> Conditions, IReadOnlyList<ValueResponse> Allergies, IReadOnlyList<ValueResponse> Medications);
    private sealed record ScenarioResponse(Guid Id, string ScenarioKey, string Name, string Setting, string SourceLanguage, string ClinicianLanguage, bool IsCanonical, int StepCount);
    private sealed record ValueResponse(string Code, string DisplayName);
    private sealed record SessionResponse(Guid Id, Guid ConsultationId);
}
