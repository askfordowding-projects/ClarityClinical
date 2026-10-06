using ClarityClinical.Domain.Assessments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClarityClinical.Infrastructure.Persistence.Configurations;

public sealed class ClinicalAssessmentConfiguration : IEntityTypeConfiguration<ClinicalAssessment>
{
    public void Configure(EntityTypeBuilder<ClinicalAssessment> builder)
    {
        builder.ToTable("clinical_assessment", DbSchemas.Clinical);
        builder.HasKey(assessment => assessment.Id);

        builder.Property(assessment => assessment.Id).HasColumnName("id");
        builder.Property(assessment => assessment.ConsultationId).HasColumnName("consultation_id");
        builder.Property(assessment => assessment.EvaluatedAt).HasColumnName("evaluated_at");

        builder.OwnsMany(assessment => assessment.Candidates, owned =>
        {
            owned.ToTable("assessment_candidate", DbSchemas.Clinical);
            owned.WithOwner().HasForeignKey("clinical_assessment_id");
            owned.Property(candidate => candidate.Key).HasColumnName("key").HasMaxLength(100);
            owned.Property(candidate => candidate.DisplayName).HasColumnName("display_name").HasMaxLength(200);
            owned.Property(candidate => candidate.CurrentScore).HasColumnName("current_score");
            owned.Property(candidate => candidate.PreviousScore).HasColumnName("previous_score");
            owned.Property(candidate => candidate.ScoreType).HasColumnName("score_type").HasConversion<string>().HasMaxLength(40);
            owned.Ignore(candidate => candidate.ChangeReasons);
            owned.HasKey("clinical_assessment_id", nameof(AssessmentCandidate.Key));
        });

        builder.Navigation(assessment => assessment.Candidates).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(assessment => new { assessment.ConsultationId, assessment.EvaluatedAt });
    }
}
