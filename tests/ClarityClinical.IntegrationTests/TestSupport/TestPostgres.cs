using System.Runtime.CompilerServices;
using Npgsql;

namespace ClarityClinical.IntegrationTests.TestSupport;

/// <summary>
/// Locates the PostgreSQL server used to create isolated per-test databases.
/// The TeamCity build platform starts its own throwaway PostgreSQL server and
/// advertises it through ConnectionStrings__ClarityClinical. Locally, the
/// default 127.0.0.1:5432 server is used.
/// </summary>
internal static class TestPostgres
{
    private const string PlatformVariable = "ConnectionStrings__ClarityClinical";
    private static string? platformConnection;

#pragma warning disable CA2255 // Intentional: must run before any test host reads environment configuration.
    [ModuleInitializer]
#pragma warning restore CA2255
    internal static void CapturePlatformDatabase()
    {
        // Capture the platform server, then remove the variable so it can never
        // override the per-test connection string given to the application.
        platformConnection = Environment.GetEnvironmentVariable(PlatformVariable);
        Environment.SetEnvironmentVariable(PlatformVariable, null);
    }

    public static string AdminConnectionString => For("postgres");

    public static string For(string databaseName)
    {
        var builder = string.IsNullOrWhiteSpace(platformConnection)
            ? new NpgsqlConnectionStringBuilder("Host=127.0.0.1;Port=5432")
            : new NpgsqlConnectionStringBuilder(platformConnection);
        builder.Username = "postgres";
        builder.Password = null;
        builder.Database = databaseName;
        return builder.ConnectionString;
    }
}
