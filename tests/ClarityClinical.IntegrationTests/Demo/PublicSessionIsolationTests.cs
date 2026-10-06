using ClarityClinical.Application.Consultations;
using ClarityClinical.Application.Consultations.Workspace;
using ClarityClinical.Application.Demo;
using ClarityClinical.Domain.Consultations;
using ClarityClinical.Infrastructure.Demo;
using ClarityClinical.Infrastructure.Persistence;
using ClarityClinical.IntegrationTests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClarityClinical.IntegrationTests.Demo;

public sealed class PublicSessionIsolationTests
{
    private const string ScenarioKey = "miguel-santos-leg-swelling";

    [Fact]
    public async Task Two_visitors_get_independent_consultations_from_the_same_canonical_scenario()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var provider = TestServiceProviderFactory.Create(database.ConnectionString);
        await using var scope = provider.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<ClarityClinicalDbContext>();
        await db.Database.MigrateAsync();
        await services.GetRequiredService<DemoScenarioSeeder>().SeedAsync(CancellationToken.None);

        var sessions = services.GetRequiredService<IDemoSessionService>();
        var visitorA = await sessions.CreateAsync(ScenarioKey, "visitor-a", CancellationToken.None);
        var visitorB = await sessions.CreateAsync(ScenarioKey, "visitor-b", CancellationToken.None);
        Assert.NotEqual(visitorA.ConsultationId, visitorB.ConsultationId);

        var lifecycle = services.GetRequiredService<ConsultationLifecycleService>();
        var now = DateTimeOffset.UtcNow;
        await lifecycle.StartAsync(visitorA.ConsultationId, now, CancellationToken.None);
        await lifecycle.StartAsync(visitorB.ConsultationId, now, CancellationToken.None);
        await services.GetRequiredService<AddClinicalFactService>().AddAsync(
            visitorA.ConsultationId,
            "unilateral-calf-swelling",
            "Unilateral calf swelling",
            ClinicalFactSource.PatientReport,
            CancellationToken.None);

        var consultations = services.GetRequiredService<IConsultationRepository>();
        var a = await consultations.GetAsync(visitorA.ConsultationId, CancellationToken.None);
        var b = await consultations.GetAsync(visitorB.ConsultationId, CancellationToken.None);
        Assert.Single(a!.Facts);
        Assert.Empty(b!.Facts);
    }

    [Fact]
    public async Task Reset_replaces_only_the_requesting_visitors_consultation_and_preserves_canonical_data()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var provider = TestServiceProviderFactory.Create(database.ConnectionString);
        await using var scope = provider.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<ClarityClinicalDbContext>();
        await db.Database.MigrateAsync();
        await services.GetRequiredService<DemoScenarioSeeder>().SeedAsync(CancellationToken.None);

        var canonicalBefore = await db.DemoScenarios.AsNoTracking().SingleAsync();
        var sessions = services.GetRequiredService<IDemoSessionService>();
        var visitorA = await sessions.CreateAsync(ScenarioKey, "visitor-a", CancellationToken.None);
        var visitorB = await sessions.CreateAsync(ScenarioKey, "visitor-b", CancellationToken.None);
        var oldAConsultationId = visitorA.ConsultationId;

        var resetA = await sessions.ResetAsync(visitorA.Id, "visitor-a", CancellationToken.None);

        Assert.NotEqual(oldAConsultationId, resetA.ConsultationId);
        Assert.Equal(visitorB.ConsultationId, (await sessions.GetAsync(visitorB.Id, "visitor-b", CancellationToken.None))!.ConsultationId);
        var canonicalAfter = await db.DemoScenarios.AsNoTracking().SingleAsync();
        Assert.Equal(canonicalBefore.Id, canonicalAfter.Id);
        Assert.Equal(canonicalBefore.ScenarioKey, canonicalAfter.ScenarioKey);
    }

    [Fact]
    public async Task Expired_sessions_remove_temporary_consultations_without_touching_canonical_scenario()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var provider = TestServiceProviderFactory.Create(database.ConnectionString);
        await using var scope = provider.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<ClarityClinicalDbContext>();
        await db.Database.MigrateAsync();
        await services.GetRequiredService<DemoScenarioSeeder>().SeedAsync(CancellationToken.None);

        var sessions = services.GetRequiredService<IDemoSessionService>();
        var session = await sessions.CreateAsync(ScenarioKey, "visitor-a", CancellationToken.None);
        session.Touch(DateTimeOffset.UtcNow.AddHours(-3), TimeSpan.FromHours(2));
        await db.SaveChangesAsync();

        var removed = await sessions.DeleteExpiredAsync(DateTimeOffset.UtcNow, CancellationToken.None);

        Assert.Equal(1, removed);
        Assert.False(await db.DemoSessions.AnyAsync(item => item.Id == session.Id));
        Assert.False(await db.Consultations.AnyAsync(item => item.Id == session.ConsultationId));
        Assert.True(await db.DemoScenarios.AnyAsync(item => item.ScenarioKey == ScenarioKey && item.IsCanonical));
    }
}
