using ClarityClinical.Domain.Transcripts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClarityClinical.Infrastructure.Persistence.Configurations;

public sealed class TranscriptSegmentConfiguration : IEntityTypeConfiguration<TranscriptSegment>
{
    public void Configure(EntityTypeBuilder<TranscriptSegment> builder)
    {
        builder.ToTable("transcript_segment", DbSchemas.Consultations);
        builder.HasKey(segment => segment.Id);

        builder.Property(segment => segment.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property(segment => segment.ConsultationId)
            .HasColumnName("consultation_id");
        builder.Property(segment => segment.SpeakerRole)
            .HasColumnName("speaker_role")
            .HasConversion<string>()
            .HasMaxLength(40);
        builder.Property(segment => segment.SourceLanguage)
            .HasColumnName("source_language")
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(segment => segment.MachineOriginalText)
            .HasColumnName("machine_original_text")
            .HasMaxLength(4000)
            .IsRequired();
        builder.Property(segment => segment.CorrectedText)
            .HasColumnName("corrected_text")
            .HasMaxLength(4000);
        builder.Property(segment => segment.TranslatedText)
            .HasColumnName("translated_text")
            .HasMaxLength(4000);
        builder.Property(segment => segment.RecognitionConfidence)
            .HasColumnName("recognition_confidence");
        builder.Property(segment => segment.OccurredAt)
            .HasColumnName("occurred_at");
        builder.Property(segment => segment.IncludeInReasoning)
            .HasColumnName("include_in_reasoning");
        builder.Property(segment => segment.IsRedacted)
            .HasColumnName("is_redacted");

        builder.Ignore(segment => segment.DisplayOriginalText);
        builder.Ignore(segment => segment.Corrected);
        builder.HasIndex(segment => new { segment.ConsultationId, segment.OccurredAt });
    }
}
