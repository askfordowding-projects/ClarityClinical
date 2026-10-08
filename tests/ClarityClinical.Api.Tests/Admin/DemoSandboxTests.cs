using System.Net.Http.Json;
using ClarityClinical.Api.Tests.TestSupport;
using ClarityClinical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClarityClinical.Api.Tests.Admin;

public sealed class DemoSandboxTests
{
    private const string ScenarioKey = "miguel-santos-leg-swelling";

    [Fact]
    public async Task Administrator_can_edit_visitor_sandbox_without_changing_canonical_patient()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        await LoginAsync(client, "Administrator");
        await context.SeedMiguelScenarioAsync();

        var create = await client.PostAsync($"/api/admin/demo/scenarios/{ScenarioKey}/sandbox", null);
        create.EnsureSuccessStatusCode();

        var update = await client.PutAsJsonAsync($"/api/admin/demo/scenarios/{ScenarioKey}/sandbox/patient", new
        {
            givenName = "Miguelito",
            familyName = "Santos",
            dateOfBirth = new DateOnly(1968, 6, 15),
            conditions = new[] { new { code = "type-2-diabetes", displayName = "Type 2 diabetes" } },
            allergies = new[] { new { code = "penicillin", displayName = "Penicillin" } },
            medications = new[] { new { code = "metformin", displayName = "Metformin" } }
        });
        update.EnsureSuccessStatusCode();

        await LoginAsync(client, "Clinician");
        var sessionResponse = await client.PostAsJsonAsync("/api/demo/sessions", new { scenarioKey = ScenarioKey });
        sessionResponse.EnsureSuccessStatusCode();
        var session = await sessionResponse.Content.ReadFromJsonAsync<SessionResponse>();
        var workspace = await client.GetFromJsonAsync<WorkspaceResponse>($"/api/consultations/{session!.ConsultationId}/workspace");
        Assert.Equal("Miguelito Santos", workspace!.Patient.DisplayName);

        await using var scope = context.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ClarityClinicalDbContext>();
        var canonical = await (from scenario in db.DemoScenarios.AsNoTracking()
                               join patient in db.Patients.AsNoTracking() on scenario.PatientId equals patient.Id
                               where scenario.ScenarioKey == ScenarioKey && scenario.IsCanonical
                               select patient).SingleAsync();
        Assert.Equal("Miguel", canonical.GivenName);
        Assert.Equal("Santos", canonical.FamilyName);
    }

    [Fact]
    public async Task Visitor_sandboxes_are_isolated()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var visitorA = context.CreateClient();
        using var visitorB = context.CreateClient();
        await LoginAsync(visitorA, "Administrator");
        await LoginAsync(visitorB, "Administrator");
        await context.SeedMiguelScenarioAsync();

        (await visitorA.PostAsync($"/api/admin/demo/scenarios/{ScenarioKey}/sandbox", null)).EnsureSuccessStatusCode();
        (await visitorA.PutAsJsonAsync($"/api/admin/demo/scenarios/{ScenarioKey}/sandbox/patient", new
        {
            givenName = "VisitorA", familyName = "Santos", dateOfBirth = new DateOnly(1968, 6, 15),
            conditions = Array.Empty<object>(), allergies = Array.Empty<object>(), medications = Array.Empty<object>()
        })).EnsureSuccessStatusCode();

        await LoginAsync(visitorA, "Clinician");
        await LoginAsync(visitorB, "Clinician");
        var aSession = await CreateSessionAsync(visitorA);
        var bSession = await CreateSessionAsync(visitorB);
        var a = await visitorA.GetFromJsonAsync<WorkspaceResponse>($"/api/consultations/{aSession.ConsultationId}/workspace");
        var b = await visitorB.GetFromJsonAsync<WorkspaceResponse>($"/api/consultations/{bSession.ConsultationId}/workspace");

        Assert.Equal("VisitorA Santos", a!.Patient.DisplayName);
        Assert.Equal("Miguel Santos", b!.Patient.DisplayName);
    }

    [Fact]
    public async Task Administrator_can_edit_sandbox_scenario_metadata_and_sessions_use_sandbox_scenario()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        await LoginAsync(client, "Administrator");
        await context.SeedMiguelScenarioAsync();
        var created = await client.PostAsync($"/api/admin/demo/scenarios/{ScenarioKey}/sandbox", null);
        created.EnsureSuccessStatusCode();
        var initial = await created.Content.ReadFromJsonAsync<SandboxResponse>();

        var update = await client.PutAsJsonAsync($"/api/admin/demo/scenarios/{ScenarioKey}/sandbox/scenario", new
        { name = "Miguel edited scenario", description = "Visitor sandbox", setting = "Community Clinic", sourceLanguage = "es", clinicianLanguage = "en" });
        update.EnsureSuccessStatusCode();
        var edited = await update.Content.ReadFromJsonAsync<SandboxResponse>();
        Assert.Equal("Community Clinic", edited!.Scenario.Setting);
        Assert.False(edited.Scenario.IsCanonical);

        await LoginAsync(client, "Clinician");
        var session = await CreateSessionAsync(client);
        Assert.Equal(edited.Scenario.Id, session.ScenarioId);
        Assert.NotEqual(initial!.SourceScenarioId, session.ScenarioId);
    }

    private static async Task LoginAsync(HttpClient client, string role) =>
        (await client.PostAsJsonAsync("/api/auth/demo-login", new { role })).EnsureSuccessStatusCode();

    private static async Task<SessionResponse> CreateSessionAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/demo/sessions", new { scenarioKey = ScenarioKey });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SessionResponse>())!;
    }

    private sealed record SessionResponse(Guid Id, Guid ScenarioId, Guid ConsultationId);
    private sealed record WorkspaceResponse(PatientResponse Patient);
    private sealed record PatientResponse(Guid Id, string DisplayName);
    private sealed record SandboxResponse(Guid Id, Guid SourceScenarioId, SandboxScenarioResponse Scenario);
    private sealed record SandboxScenarioResponse(Guid Id, string ScenarioKey, string Name, string Setting, bool IsCanonical);
}
