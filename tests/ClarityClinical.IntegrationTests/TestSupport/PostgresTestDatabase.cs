using Npgsql;

namespace ClarityClinical.IntegrationTests.TestSupport;

public sealed class PostgresTestDatabase : IAsyncDisposable
{

    private readonly NpgsqlConnection _adminConnection;
    private readonly string _databaseName;

    private PostgresTestDatabase(NpgsqlConnection adminConnection, string databaseName)
    {
        _adminConnection = adminConnection;
        _databaseName = databaseName;
        ConnectionString = TestPostgres.For(databaseName);
    }

    public string ConnectionString { get; }

    public static async Task<PostgresTestDatabase> CreateAsync()
    {
        var databaseName = $"clarityclinical_test_{Guid.NewGuid():N}";
        var adminConnection = new NpgsqlConnection(TestPostgres.AdminConnectionString);
        await adminConnection.OpenAsync();

        await using var command = new NpgsqlCommand(
            $"CREATE DATABASE \"{databaseName}\"",
            adminConnection);
        await command.ExecuteNonQueryAsync();

        return new PostgresTestDatabase(adminConnection, databaseName);
    }

    public async ValueTask DisposeAsync()
    {
        await using var command = new NpgsqlCommand(
            $"DROP DATABASE IF EXISTS \"{_databaseName}\" WITH (FORCE)",
            _adminConnection);
        await command.ExecuteNonQueryAsync();
        await _adminConnection.DisposeAsync();
    }
}
