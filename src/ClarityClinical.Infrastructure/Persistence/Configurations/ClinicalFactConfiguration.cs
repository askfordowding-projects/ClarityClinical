using ClarityClinical.Domain.Consultations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClarityClinical.Infrastructure.Persistence.Configurations;

public sealed class ClinicalFactConfiguration : IEntityTypeConfiguration<ClinicalFact>
{
    public void Configure(EntityTypeBuilder<ClinicalFact> builder)
    {
        builder.ToTable("clinical_event", DbSchemas.Consultations);
        builder.HasKey(fact => fact.Id);

        builder.Property(fact => fact.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property<Guid>("consultation_id").HasColumnName("consultation_id");
        builder.Property(fact => fact.Code).HasColumnName("code").HasMaxLength(120).IsRequired();
        builder.Property(fact => fact.DisplayText).HasColumnName("display_text").HasMaxLength(500).IsRequired();
        builder.Property(fact => fact.Source).HasColumnName("source_type").HasConversion<string>().HasMaxLength(40);
        builder.Property(fact => fact.OccurredAt).HasColumnName("occurred_at");
        builder.Property(fact => fact.SourceEventId).HasColumnName("source_event_id");
        builder.Property(fact => fact.IncludeInReasoning).HasColumnName("include_in_reasoning");

        builder.HasIndex("consultation_id", nameof(ClinicalFact.OccurredAt));
        builder.HasIndex(fact => fact.SourceEventId);
    }
}
