using ClarityClinical.Domain.Demo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClarityClinical.Infrastructure.Persistence.Configurations;

public sealed class DemoScenarioStepConfiguration : IEntityTypeConfiguration<DemoScenarioStep>
{
    public void Configure(EntityTypeBuilder<DemoScenarioStep> builder)
    {
        builder.ToTable("demo_scenario_step", DbSchemas.Admin);
        builder.HasKey(step => step.Id);

        builder.Property(step => step.Id).HasColumnName("id");
        builder.Property(step => step.ScenarioId).HasColumnName("scenario_id");
        builder.Property(step => step.Sequence).HasColumnName("sequence");
        builder.Property(step => step.StepType).HasColumnName("step_type").HasConversion<string>().HasMaxLength(40);
        builder.Property(step => step.ClinicalFactCode).HasColumnName("clinical_fact_code").HasMaxLength(120).IsRequired();
        builder.Property(step => step.ClinicalFactDisplayText).HasColumnName("clinical_fact_display_text").HasMaxLength(300).IsRequired();
        builder.Property(step => step.OriginalText).HasColumnName("original_text").HasMaxLength(1000);
        builder.Property(step => step.SourceLanguage).HasColumnName("source_language").HasMaxLength(16);

        builder.HasIndex(step => new { step.ScenarioId, step.Sequence }).IsUnique();
    }
}
