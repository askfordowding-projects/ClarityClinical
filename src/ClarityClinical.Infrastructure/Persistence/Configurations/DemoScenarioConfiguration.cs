using ClarityClinical.Domain.Demo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClarityClinical.Infrastructure.Persistence.Configurations;

public sealed class DemoScenarioConfiguration : IEntityTypeConfiguration<DemoScenario>
{
    public void Configure(EntityTypeBuilder<DemoScenario> builder)
    {
        builder.ToTable("demo_scenario", DbSchemas.Admin);
        builder.HasKey(scenario => scenario.Id);

        builder.Property(scenario => scenario.Id).HasColumnName("id");
        builder.Property(scenario => scenario.ScenarioKey).HasColumnName("scenario_key").HasMaxLength(120).IsRequired();
        builder.Property(scenario => scenario.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(scenario => scenario.Description).HasColumnName("description").HasMaxLength(1000).IsRequired();
        builder.Property(scenario => scenario.PatientId).HasColumnName("patient_id");
        builder.Property(scenario => scenario.Setting).HasColumnName("setting").HasMaxLength(120).IsRequired();
        builder.Property(scenario => scenario.SourceLanguage).HasColumnName("source_language").HasMaxLength(16).IsRequired();
        builder.Property(scenario => scenario.ClinicianLanguage).HasColumnName("clinician_language").HasMaxLength(16).IsRequired();
        builder.Property(scenario => scenario.IsCanonical).HasColumnName("is_canonical");

        builder.HasIndex(scenario => scenario.ScenarioKey).IsUnique();
        builder.HasMany(scenario => scenario.Steps)
            .WithOne()
            .HasForeignKey(step => step.ScenarioId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(scenario => scenario.Steps).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
