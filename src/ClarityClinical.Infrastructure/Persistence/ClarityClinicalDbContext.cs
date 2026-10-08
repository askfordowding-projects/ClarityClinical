using ClarityClinical.Domain.Assessments;
using ClarityClinical.Domain.Audit;
using ClarityClinical.Domain.Consultations;
using ClarityClinical.Domain.Demo;
using ClarityClinical.Domain.Governance;
using ClarityClinical.Domain.Patients;
using ClarityClinical.Domain.Transcripts;
using Microsoft.EntityFrameworkCore;

namespace ClarityClinical.Infrastructure.Persistence;

public sealed class ClarityClinicalDbContext(DbContextOptions<ClarityClinicalDbContext> options)
    : DbContext(options)
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Consultation> Consultations => Set<Consultation>();
    public DbSet<ClinicalAssessment> ClinicalAssessments => Set<ClinicalAssessment>();
    public DbSet<ClinicianResponse> ClinicianResponses => Set<ClinicianResponse>();
    public DbSet<DemoScenario> DemoScenarios => Set<DemoScenario>();
    public DbSet<DemoScenarioStep> DemoScenarioSteps => Set<DemoScenarioStep>();
    public DbSet<DemoSession> DemoSessions => Set<DemoSession>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<TranscriptSegment> TranscriptSegments => Set<TranscriptSegment>();
    public DbSet<GuidelineSource> GuidelineSources => Set<GuidelineSource>();
    public DbSet<ClinicalRuleVersion> ClinicalRuleVersions => Set<ClinicalRuleVersion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClarityClinicalDbContext).Assembly);
    }
}
