using ClarityClinical.Application.Demo;
using ClarityClinical.Infrastructure.Demo;
using ClarityClinical.Infrastructure.Persistence;
using ClarityClinical.IntegrationTests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClarityClinical.IntegrationTests.Demo;

public sealed class MiguelScenarioSeedTests
{
    private const string ScenarioKey = "miguel-santos-leg-swelling";

    [Fact]
    public async Task Canonical_Miguel_scenario_contains_expected_synthetic_patient_context()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var serviceProvider = TestServiceProviderFactory.Create(database.ConnectionString);
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ClarityClinicalDbContext>();
        await dbContext.Database.MigrateAsync();

        var seeder = scope.ServiceProvider.GetRequiredService<DemoScenarioSeeder>();
        await seeder.SeedAsync(CancellationToken.None);
        var repository = scope.ServiceProvider.GetRequiredService<IDemoScenarioRepository>();

        var scenario = await repository.GetCanonicalScenarioAsync(ScenarioKey, CancellationToken.None);
        Assert.NotNull(scenario);
        Assert.Equal("Urgent Treatment Centre", scenario.Setting);
        Assert.Equal("es", scenario.SourceLanguage);
        Assert.Equal("en", scenario.ClinicianLanguage);

        var patient = await dbContext.Patients.SingleAsync(patient => patient.Id == scenario.PatientId);
        Assert.Equal("Miguel Santos", patient.DisplayName);
        Assert.Equal(58, AgeOn(patient.DateOfBirth, new DateOnly(2026, 10, 6)));
        Assert.Contains(patient.Allergies, allergy => allergy.DisplayName == "Penicillin");
        Assert.Contains(patient.Allergies, allergy => allergy.DisplayName == "Ibuprofen");
        Assert.Contains(patient.Conditions, condition => condition.DisplayName == "Type 2 diabetes");
        Assert.Contains(patient.Conditions, condition => condition.DisplayName == "Hypertension");
        Assert.Contains(patient.Conditions, condition => condition.DisplayName == "Asthma");
        Assert.Contains(patient.Medications, medication => medication.DisplayName == "Metformin");
        Assert.Contains(patient.Medications, medication => medication.DisplayName == "Ramipril");
        Assert.Contains(patient.Medications, medication => medication.DisplayName == "Paracetamol");
        Assert.Contains(patient.Medications, medication => medication.DisplayName == "Salbutamol");
        Assert.NotEmpty(scenario.Steps);
    }

    [Fact]
    public async Task Seeding_twice_does_not_duplicate_or_mutate_canonical_scenario()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var serviceProvider = TestServiceProviderFactory.Create(database.ConnectionString);
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ClarityClinicalDbContext>();
        await dbContext.Database.MigrateAsync();

        var seeder = scope.ServiceProvider.GetRequiredService<DemoScenarioSeeder>();
        await seeder.SeedAsync(CancellationToken.None);
        var first = await dbContext.DemoScenarios.AsNoTracking().SingleAsync();
        var firstStepCount = await dbContext.DemoScenarioSteps.CountAsync();

        await seeder.SeedAsync(CancellationToken.None);

        var second = await dbContext.DemoScenarios.AsNoTracking().SingleAsync();
        Assert.Equal(first.Id, second.Id);
        Assert.Equal(first.ScenarioKey, second.ScenarioKey);
        Assert.Equal(firstStepCount, await dbContext.DemoScenarioSteps.CountAsync());
        Assert.Single(await dbContext.DemoScenarios.AsNoTracking().ToListAsync());
    }

    private static int AgeOn(DateOnly dateOfBirth, DateOnly date)
    {
        var age = date.Year - dateOfBirth.Year;
        return date < dateOfBirth.AddYears(age) ? age - 1 : age;
    }
}
