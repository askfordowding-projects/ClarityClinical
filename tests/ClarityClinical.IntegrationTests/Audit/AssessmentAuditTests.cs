using ClarityClinical.Application.ClinicalIntelligence;
using ClarityClinical.Application.Consultations;
using ClarityClinical.Application.Consultations.Workspace;
using ClarityClinical.Application.Demo;
using ClarityClinical.Domain.Consultations;
using ClarityClinical.Infrastructure.Demo;
using ClarityClinical.Infrastructure.Persistence;
using ClarityClinical.IntegrationTests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClarityClinical.IntegrationTests.Audit;

public sealed class AssessmentAuditTests
{
    [Fact]
    public async Task Adding_clinical_evidence_records_fact_rule_and_assessment_audit_events()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var provider = TestServiceProviderFactory.Create(database.ConnectionString);
        await using var scope = provider.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<ClarityClinicalDbContext>();
        await db.Database.MigrateAsync();
        await services.GetRequiredService<DemoScenarioSeeder>().SeedAsync(CancellationToken.None);

        var session = await services.GetRequiredService<IDemoSessionService>().CreateAsync(
            "miguel-santos-leg-swelling",
            "visitor-a",
            CancellationToken.None);
        await services.GetRequiredService<ConsultationLifecycleService>().StartAsync(
            session.ConsultationId,
            DateTimeOffset.UtcNow,
            CancellationToken.None);

        var facts = services.GetRequiredService<AddClinicalFactService>();
        await facts.AddAsync(
            session.ConsultationId,
            "unilateral-calf-swelling",
            "Unilateral calf swelling",
            ClinicalFactSource.PatientReport,
            CancellationToken.None);
        await facts.AddAsync(
            session.ConsultationId,
            "prolonged-travel",
            "Recent prolonged travel",
            ClinicalFactSource.PatientReport,
            CancellationToken.None);

        var events = await db.AuditEvents
            .AsNoTracking()
            .Where(item => item.ConsultationId == session.ConsultationId)
            .OrderBy(item => item.OccurredAt)
            .ToListAsync();

        Assert.Contains(events, item => item.Action == "ClinicalFactAdded");
        Assert.Contains(events, item => item.Action == "RuleEvaluated" && item.RuleVersion == MiguelScenarioRuleSet.Version);
        Assert.Contains(events, item => item.Action == "AssessmentChanged" && item.RuleVersion == MiguelScenarioRuleSet.Version);
        Assert.All(events, item => Assert.False(string.IsNullOrWhiteSpace(item.CorrelationId)));
    }
}
