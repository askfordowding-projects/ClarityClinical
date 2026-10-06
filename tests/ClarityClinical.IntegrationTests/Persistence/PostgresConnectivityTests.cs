using ClarityClinical.Infrastructure.Persistence;
using ClarityClinical.IntegrationTests.TestSupport;
using Microsoft.Extensions.DependencyInjection;

namespace ClarityClinical.IntegrationTests.Persistence;

public sealed class PostgresConnectivityTests
{
    [Fact]
    public async Task Database_can_connect_and_report_postgresql_provider()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var serviceProvider = TestServiceProviderFactory.Create(database.ConnectionString);
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ClarityClinicalDbContext>();

        Assert.True(await dbContext.Database.CanConnectAsync());
        Assert.Contains("Npgsql", dbContext.Database.ProviderName, StringComparison.Ordinal);
    }
}
