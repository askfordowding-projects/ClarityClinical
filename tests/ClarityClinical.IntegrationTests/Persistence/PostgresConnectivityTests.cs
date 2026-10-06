using ClarityClinical.Api.Configuration;
using ClarityClinical.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace ClarityClinical.IntegrationTests.Persistence;

public sealed class PostgresConnectivityTests
{
    [Fact]
    public async Task Database_can_connect_and_report_postgresql_provider()
    {
        var databaseName = $"clarityclinical_test_{Guid.NewGuid():N}";
        const string adminConnectionString = "Host=127.0.0.1;Port=5432;Username=postgres;Database=postgres";

        await using var adminConnection = new NpgsqlConnection(adminConnectionString);
        await adminConnection.OpenAsync();
        await using (var createCommand = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", adminConnection))
        {
            await createCommand.ExecuteNonQueryAsync();
        }

        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:ClarityClinical"] =
                        $"Host=127.0.0.1;Port=5432;Username=postgres;Database={databaseName}"
                })
                .Build();

            var services = new ServiceCollection();
            services.AddClarityClinicalPersistence(configuration);

            await using var serviceProvider = services.BuildServiceProvider();
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ClarityClinicalDbContext>();

            Assert.True(await dbContext.Database.CanConnectAsync());
            Assert.Contains("Npgsql", dbContext.Database.ProviderName, StringComparison.Ordinal);
        }
        finally
        {
            await using var dropCommand = new NpgsqlCommand(
                $"DROP DATABASE IF EXISTS \"{databaseName}\" WITH (FORCE)",
                adminConnection);
            await dropCommand.ExecuteNonQueryAsync();
        }
    }
}
