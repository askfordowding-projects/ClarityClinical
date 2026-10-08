using System.Net;
using System.Net.Http.Json;
using ClarityClinical.Api.Tests.TestSupport;
using ClarityClinical.Domain.Audit;
using ClarityClinical.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace ClarityClinical.Api.Tests.Admin;

public sealed class GovernanceAdminTests
{
    [Fact]
    public async Task Clinician_cannot_read_admin_governance()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        (await client.PostAsJsonAsync("/api/auth/demo-login", new { role = "Clinician" })).EnsureSuccessStatusCode();

        var response = await client.GetAsync("/api/admin/governance/rules");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Administrator_can_inspect_active_rule_provenance_without_implying_validated_probabilities()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        (await client.PostAsJsonAsync("/api/auth/demo-login", new { role = "Administrator" })).EnsureSuccessStatusCode();

        var response = await client.GetAsync("/api/admin/governance/rules");

        response.EnsureSuccessStatusCode();
        var rules = await response.Content.ReadFromJsonAsync<IReadOnlyList<RuleResponse>>();
        Assert.NotNull(rules);
        Assert.Equal(3, rules!.Count);

        var dvt = Assert.Single(rules, item => item.RuleKey == "dvt");
        Assert.Equal("demo-illustrative-1.0", dvt.Version);
        Assert.Equal("IllustrativePriority", dvt.ScoreType);
        Assert.True(dvt.IsIllustrative);
        Assert.Equal("NG158", dvt.Source.ReferenceCode);
        Assert.Equal("NICE", dvt.Source.Organisation);
        Assert.Contains("not", dvt.ProvenanceNote, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("validated", dvt.ProvenanceNote, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Administrator_can_inspect_guideline_sources()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        (await client.PostAsJsonAsync("/api/auth/demo-login", new { role = "Administrator" })).EnsureSuccessStatusCode();

        var response = await client.GetAsync("/api/admin/governance/guidelines");

        response.EnsureSuccessStatusCode();
        var sources = await response.Content.ReadFromJsonAsync<IReadOnlyList<GuidelineResponse>>();
        Assert.NotNull(sources);
        Assert.Contains(sources!, source =>
            source.ReferenceCode == "NG158" &&
            source.PublishedDate == new DateOnly(2020, 3, 26));
        Assert.Contains(sources!, source =>
            source.ReferenceCode == "NG141" &&
            source.PublishedDate == new DateOnly(2019, 9, 27));
    }

    [Fact]
    public async Task Public_admin_audit_is_isolated_to_the_current_visitor_sessions()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var adminA = context.CreateClient();
        using var adminB = context.CreateClient();
        (await adminA.PostAsJsonAsync("/api/auth/demo-login", new { role = "Administrator" })).EnsureSuccessStatusCode();
        (await adminB.PostAsJsonAsync("/api/auth/demo-login", new { role = "Administrator" })).EnsureSuccessStatusCode();
        await context.SeedMiguelScenarioAsync();

        var sessionA = await CreateSessionAsync(adminA);
        var sessionB = await CreateSessionAsync(adminB);
        await SeedAuditAsync(context, sessionA, "VisitorAEvent");
        await SeedAuditAsync(context, sessionB, "VisitorBEvent");

        var response = await adminA.GetAsync("/api/admin/audit");

        response.EnsureSuccessStatusCode();
        var audit = await response.Content.ReadFromJsonAsync<AuditPageResponse>();
        Assert.NotNull(audit);
        Assert.Contains(audit!.Items, item => item.Action == "VisitorAEvent");
        Assert.DoesNotContain(audit.Items, item => item.Action == "VisitorBEvent");
    }

    private static async Task<DemoSessionResponse> CreateSessionAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/demo/sessions",
            new { scenarioKey = "miguel-santos-leg-swelling" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<DemoSessionResponse>())!;
    }

    private static async Task SeedAuditAsync(
        ApiTestContext context,
        DemoSessionResponse session,
        string action)
    {
        await using var scope = context.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ClarityClinicalDbContext>();
        db.AuditEvents.Add(new AuditEvent(
            Guid.NewGuid(),
            session.Id,
            session.ConsultationId,
            "test-actor",
            "Administrator",
            action,
            "TestEntity",
            Guid.NewGuid().ToString(),
            DateTimeOffset.UtcNow,
            Guid.NewGuid().ToString("N")));
        await db.SaveChangesAsync();
    }

    private sealed record DemoSessionResponse(Guid Id, Guid ConsultationId);
    private sealed record RuleResponse(
        string RuleKey,
        string DisplayName,
        string Version,
        string Status,
        string ScoreType,
        bool IsIllustrative,
        string ProvenanceNote,
        GuidelineResponse Source);
    private sealed record GuidelineResponse(
        string ReferenceCode,
        string Organisation,
        string Title,
        string? Url,
        DateOnly PublishedDate,
        DateOnly? LastUpdatedDate,
        DateOnly? LastReviewedDate,
        string ReviewStatus);
    private sealed record AuditPageResponse(int TotalCount, IReadOnlyList<AuditResponse> Items);
    private sealed record AuditResponse(
        Guid Id,
        Guid ConsultationId,
        string ActorRole,
        string Action,
        string EntityType,
        string EntityId,
        DateTimeOffset OccurredAt,
        string CorrelationId,
        string? PreviousState,
        string? NewState,
        string? Reason,
        string? RuleVersion);
}

