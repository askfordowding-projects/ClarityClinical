using ClarityClinical.Application.Demo;
using ClarityClinical.Infrastructure.Demo;
using ClarityClinical.Infrastructure.Persistence;
using ClarityClinical.IntegrationTests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClarityClinical.IntegrationTests.Demo;

public sealed class DemoSandboxCleanupTests
{
    [Fact]
    public async Task Expired_inactive_sandbox_is_removed_without_touching_canonical_data()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var provider = TestServiceProviderFactory.Create(database.ConnectionString);
        await using var scope = provider.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<ClarityClinicalDbContext>();
        await db.Database.MigrateAsync();
        await services.GetRequiredService<DemoScenarioSeeder>().SeedAsync(CancellationToken.None);

        var sandboxes = services.GetRequiredService<IDemoSandboxService>();
        var snapshot = await sandboxes.GetOrCreateAsync("miguel-santos-leg-swelling", "visitor-a", CancellationToken.None);
        var entity = await db.DemoSandboxes.SingleAsync(x => x.Id == snapshot.Id);
        entity.Touch(DateTimeOffset.UtcNow.AddHours(-3));
        await db.SaveChangesAsync();

        var removed = await sandboxes.DeleteExpiredAsync(DateTimeOffset.UtcNow, TimeSpan.FromHours(2), CancellationToken.None);

        Assert.Equal(1, removed);
        Assert.False(await db.DemoSandboxes.AnyAsync(x => x.Id == snapshot.Id));
        Assert.False(await db.DemoScenarios.AnyAsync(x => x.Id == snapshot.Scenario.Id));
        Assert.False(await db.Patients.AnyAsync(x => x.Id == snapshot.Patient.Id));
        Assert.True(await db.DemoScenarios.AnyAsync(x => x.ScenarioKey == "miguel-santos-leg-swelling" && x.IsCanonical));
    }

    [Fact]
    public async Task Active_session_prevents_its_sandbox_from_being_cleaned()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var provider = TestServiceProviderFactory.Create(database.ConnectionString);
        await using var scope = provider.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<ClarityClinicalDbContext>();
        await db.Database.MigrateAsync();
        await services.GetRequiredService<DemoScenarioSeeder>().SeedAsync(CancellationToken.None);

        var sandboxes = services.GetRequiredService<IDemoSandboxService>();
        var snapshot = await sandboxes.GetOrCreateAsync("miguel-santos-leg-swelling", "visitor-a", CancellationToken.None);
        var entity = await db.DemoSandboxes.SingleAsync(x => x.Id == snapshot.Id);
        entity.Touch(DateTimeOffset.UtcNow.AddHours(-3));
        await db.SaveChangesAsync();
        await services.GetRequiredService<IDemoSessionService>().CreateAsync("miguel-santos-leg-swelling", "visitor-a", CancellationToken.None);

        var removed = await sandboxes.DeleteExpiredAsync(DateTimeOffset.UtcNow, TimeSpan.FromHours(2), CancellationToken.None);

        Assert.Equal(0, removed);
        Assert.True(await db.DemoSandboxes.AnyAsync(x => x.Id == snapshot.Id));
    }
}
