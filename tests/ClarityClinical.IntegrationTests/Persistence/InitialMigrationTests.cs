using ClarityClinical.Infrastructure.Persistence;
using ClarityClinical.IntegrationTests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace ClarityClinical.IntegrationTests.Persistence;

public sealed class InitialMigrationTests
{
    [Fact]
    public async Task Initial_migration_creates_expected_tables_in_expected_schemas()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var serviceProvider = TestServiceProviderFactory.Create(database.ConnectionString);
        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ClarityClinicalDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        var tables = new HashSet<string>(StringComparer.Ordinal);
        await using var connection = new NpgsqlConnection(database.ConnectionString);
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
}
