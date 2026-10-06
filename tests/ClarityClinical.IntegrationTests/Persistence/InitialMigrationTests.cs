using ClarityClinical.Api.Configuration;
using ClarityClinical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace ClarityClinical.IntegrationTests.Persistence;

public sealed class InitialMigrationTests
{
    [Fact]
    public async Task Initial_migration_creates_expected_tables_in_expected_schemas()
    {
        var databaseName = $"clarityclinical_test_{Guid.NewGuid():N}";
        const string adminConnectionString = "Host=127.0.0.1;Port=5432;Username=postgres;Database=postgres";
        var testConnectionString =
            $"Host=127.0.0.1;Port=5432;Username=postgres;Database={databaseName}";

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
                    ["ConnectionStrings:ClarityClinical"] = testConnectionString
                })
                .Build();

            var services = new ServiceCollection();
            services.AddClarityClinicalPersistence(configuration);

            await using (var serviceProvider = services.BuildServiceProvider())
            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ClarityClinicalDbContext>();
                await dbContext.Database.MigrateAsync();
            }

            var tables = new HashSet<string>(StringComparer.Ordinal);
            await using var connection = new NpgsqlConnection(testConnectionString);
            await connection.OpenAsync();
            const string sql = """
                SELECT table_schema || '.' || table_name
                FROM information_schema.tables
                WHERE table_schema IN ('patients', 'consultations', 'clinical')
                """;
            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
            }

            Assert.Contains("patients.patient", tables);
            Assert.Contains("consultations.consultation", tables);
            Assert.Contains("consultations.clinical_event", tables);
            Assert.Contains("clinical.clinical_assessment", tables);
            Assert.Contains("clinical.assessment_candidate", tables);
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
