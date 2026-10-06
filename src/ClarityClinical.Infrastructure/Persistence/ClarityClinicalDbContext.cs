using ClarityClinical.Domain.Assessments;
using ClarityClinical.Domain.Consultations;
using ClarityClinical.Domain.Patients;
using Microsoft.EntityFrameworkCore;

namespace ClarityClinical.Infrastructure.Persistence;

public sealed class ClarityClinicalDbContext(DbContextOptions<ClarityClinicalDbContext> options)
    : DbContext(options)
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Consultation> Consultations => Set<Consultation>();
    public DbSet<ClinicalAssessment> ClinicalAssessments => Set<ClinicalAssessment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClarityClinicalDbContext).Assembly);
    }
}
