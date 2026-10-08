using ClarityClinical.Application.Admin;
using ClarityClinical.Application.Audit;
using ClarityClinical.Application.ClinicalIntelligence;
using ClarityClinical.Application.Consultations;
using ClarityClinical.Application.Consultations.Responses;
using ClarityClinical.Application.Consultations.Workspace;
using ClarityClinical.Application.Demo;
using ClarityClinical.Application.Identity;
using ClarityClinical.Application.Patients;
using ClarityClinical.Application.Transcripts;
using ClarityClinical.Infrastructure.Audit;
using ClarityClinical.Infrastructure.Admin;
using ClarityClinical.Infrastructure.Consultations;
using ClarityClinical.Infrastructure.Consultations.Responses;
using ClarityClinical.Infrastructure.Demo;
using ClarityClinical.Infrastructure.Identity;
using ClarityClinical.Infrastructure.Patients;
using ClarityClinical.Infrastructure.Persistence;
using ClarityClinical.Infrastructure.Transcripts;
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
        services.AddScoped<IDemoSandboxService, DemoSandboxService>();
        services.AddScoped<DemoScenarioSeeder>();
        services.AddScoped<IDemoIdentityProvider, DemoIdentityProvider>();
        services.AddScoped<ICurrentUserAccessor, SystemCurrentUserAccessor>();
        services.AddScoped<IAuditWriter, AuditWriter>();
        services.AddScoped<IAdminReadService, AdminReadService>();
        services.AddScoped<IDemoAdminReadService, DemoAdminReadService>();
        services.AddScoped<GovernanceSeeder>();
        services.AddScoped<IConsultationRepository, ConsultationRepository>();
        services.AddScoped<IClinicianResponseRepository, ClinicianResponseRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<ITranscriptSegmentRepository, TranscriptSegmentRepository>();
        services.AddScoped<ConsultationLifecycleService>();
        services.AddScoped<RecordClinicianResponseService>();
        services.AddScoped<GetConsultationWorkspaceQuery>();
        services.AddScoped<AddClinicalFactService>();
        services.AddScoped<ExcludeClinicalFactService>();
        services.AddScoped<AddTranscriptSegmentService>();
        services.AddScoped<ManageTranscriptSegmentService>();
        services.AddSingleton<TranscriptClinicalFactExtractor>();
        services.AddScoped<TranscriptClinicalFactProjector>();
        services.AddSingleton<MiguelScenarioRuleSet>();
        services.AddScoped<IClinicalIntelligenceProvider, DeterministicClinicalIntelligenceProvider>();

        return services;
    }
}
