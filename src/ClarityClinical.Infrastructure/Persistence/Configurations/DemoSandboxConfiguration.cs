using ClarityClinical.Domain.Demo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClarityClinical.Infrastructure.Persistence.Configurations;

public sealed class DemoSandboxConfiguration : IEntityTypeConfiguration<DemoSandbox>
{
    public void Configure(EntityTypeBuilder<DemoSandbox> builder)
    {
        builder.ToTable("demo_sandbox", DbSchemas.Admin);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.VisitorId).HasColumnName("visitor_id").HasMaxLength(100).IsRequired();
        builder.Property(x => x.SourceScenarioId).HasColumnName("source_scenario_id");
        builder.Property(x => x.SandboxScenarioId).HasColumnName("sandbox_scenario_id");
        builder.Property(x => x.SandboxPatientId).HasColumnName("sandbox_patient_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.HasIndex(x => new { x.VisitorId, x.SourceScenarioId }).IsUnique();
        builder.HasIndex(x => x.SandboxScenarioId).IsUnique();
        builder.HasIndex(x => x.SandboxPatientId).IsUnique();
        builder.HasOne<DemoScenario>().WithMany().HasForeignKey(x => x.SourceScenarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<DemoScenario>().WithMany().HasForeignKey(x => x.SandboxScenarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ClarityClinical.Domain.Patients.Patient>().WithMany().HasForeignKey(x => x.SandboxPatientId).OnDelete(DeleteBehavior.Restrict);
    }
}
