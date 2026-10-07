using System.Net.Http.Json;
using ClarityClinical.Infrastructure.Demo;
using ClarityClinical.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace ClarityClinical.Api.Tests.TestSupport;

public sealed class ApiTestContext : IAsyncDisposable
{
    private const string AdminConnectionString =
        "Host=127.0.0.1;Port=5432;Username=postgres;Database=postgres";

    private readonly NpgsqlConnection _adminConnection;
    private readonly string _databaseName;

    private ApiTestContext(
        NpgsqlConnection adminConnection,
        string databaseName,
        ClarityClinicalWebApplicationFactory factory)
    {
        _adminConnection = adminConnection;
        _databaseName = databaseName;
        Factory = factory;
    }

    public ClarityClinicalWebApplicationFactory Factory { get; }

    public static async Task<ApiTestContext> CreateAsync(Action<IServiceCollection>? configureServices = null)
    {
        var databaseName = $"clarityclinical_api_test_{Guid.NewGuid():N}";
        var adminConnection = new NpgsqlConnection(AdminConnectionString);
        await adminConnection.OpenAsync();
        await using (var command = new NpgsqlCommand(
            $"CREATE DATABASE \"{databaseName}\"",
            adminConnection))
        {
            await command.ExecuteNonQueryAsync();
        }

        var connectionString =
            $"Host=127.0.0.1;Port=5432;Username=postgres;Database={databaseName}";
        var factory = new ClarityClinicalWebApplicationFactory(connectionString, configureServices);

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ClarityClinicalDbContext>();
        await dbContext.Database.MigrateAsync();

        return new ApiTestContext(adminConnection, databaseName, factory);
    }

    public HttpClient CreateClient() => Factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false,
        HandleCookies = true
    });

    public async Task<Guid> SeedMiguelScenarioAsync()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DemoScenarioSeeder>();
        await seeder.SeedAsync(CancellationToken.None);
        var dbContext = scope.ServiceProvider.GetRequiredService<ClarityClinicalDbContext>();
        return await dbContext.DemoScenarios
            .Where(scenario => scenario.ScenarioKey == DemoScenarioSeeder.MiguelScenarioKey)
            .Select(scenario => scenario.PatientId)
            .SingleAsync();
    }

    public async Task<Guid> CreateMiguelDemoConsultationAsync(HttpClient client)
    {
        await SeedMiguelScenarioAsync();
        var response = await client.PostAsJsonAsync(
            "/api/demo/sessions",
            new { scenarioKey = DemoScenarioSeeder.MiguelScenarioKey });
        response.EnsureSuccessStatusCode();
        var session = await response.Content.ReadFromJsonAsync<DemoSessionResponse>();
        return session!.ConsultationId;
    }

    public async Task<Guid> AddConsultationAsync(Guid? patientId = null)
    {
        var id = Guid.NewGuid();
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ClarityClinicalDbContext>();
        dbContext.Consultations.Add(new Domain.Consultations.Consultation(
            id,
            patientId ?? Guid.NewGuid()));
        await dbContext.SaveChangesAsync();
        return id;
    }

    private sealed record DemoSessionResponse(Guid Id, Guid ConsultationId);

    public async ValueTask DisposeAsync()
    {
        await Factory.DisposeAsync();
        await using var command = new NpgsqlCommand(
            $"DROP DATABASE IF EXISTS \"{_databaseName}\" WITH (FORCE)",
            _adminConnection);
        await command.ExecuteNonQueryAsync();
        await _adminConnection.DisposeAsync();
    }
}

public sealed class ClarityClinicalWebApplicationFactory(
    string connectionString,
    Action<IServiceCollection>? configureServices = null)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:ClarityClinical", connectionString);
        if (configureServices is not null)
        {
            builder.ConfigureServices(configureServices);
        }
    }
}
