using Npgsql;

namespace ClarityClinical.IntegrationTests.TestSupport;

public sealed class PostgresTestDatabase : IAsyncDisposable
{
    private const string AdminConnectionString =
        "Host=127.0.0.1;Port=5432;Username=postgres;Database=postgres";

    private readonly NpgsqlConnection _adminConnection;
    private readonly string _databaseName;

    private PostgresTestDatabase(NpgsqlConnection adminConnection, string databaseName)
    {
        _adminConnection = adminConnection;
        _databaseName = databaseName;
        ConnectionString =
            $"Host=127.0.0.1;Port=5432;Username=postgres;Database={databaseName}";
    }

    public string ConnectionString { get; }

    public static async Task<PostgresTestDatabase> CreateAsync()
    {
        var databaseName = $"clarityclinical_test_{Guid.NewGuid():N}";
        var adminConnection = new NpgsqlConnection(AdminConnectionString);
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
