using ClarityClinical.Application.Audit;
using ClarityClinical.Application.ClinicalIntelligence;
using ClarityClinical.Application.Consultations;
using ClarityClinical.Application.Consultations.Workspace;
using ClarityClinical.Application.Demo;
using ClarityClinical.Application.Identity;
using ClarityClinical.Application.Patients;
using ClarityClinical.Infrastructure.Audit;
using ClarityClinical.Infrastructure.Consultations;
using ClarityClinical.Infrastructure.Demo;
using ClarityClinical.Infrastructure.Identity;
using ClarityClinical.Infrastructure.Patients;
using ClarityClinical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClarityClinical.Api.Configuration;

public static class PersistenceExtensions
{
    public static IServiceCollection AddClarityClinicalPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ClarityClinical")
            ?? throw new InvalidOperationException(
                "Connection string 'ClarityClinical' is not configured.");

        services.AddDbContext<ClarityClinicalDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<IDemoScenarioRepository, DemoScenarioRepository>();
        services.AddScoped<IDemoSessionService, DemoSessionService>();
        services.AddScoped<DemoScenarioSeeder>();
        services.AddScoped<IDemoIdentityProvider, DemoIdentityProvider>();
        services.AddScoped<ICurrentUserAccessor, SystemCurrentUserAccessor>();
        services.AddScoped<IAuditWriter, AuditWriter>();
        services.AddScoped<IConsultationRepository, ConsultationRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<ConsultationLifecycleService>();
        services.AddScoped<GetConsultationWorkspaceQuery>();
        services.AddScoped<AddClinicalFactService>();
        services.AddSingleton<MiguelScenarioRuleSet>();
        services.AddScoped<IClinicalIntelligenceProvider, DeterministicClinicalIntelligenceProvider>();

        return services;
    }
}
