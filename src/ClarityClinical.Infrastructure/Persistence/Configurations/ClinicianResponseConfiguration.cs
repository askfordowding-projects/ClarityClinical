using ClarityClinical.Domain.Assessments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClarityClinical.Infrastructure.Persistence.Configurations;

public sealed class ClinicianResponseConfiguration : IEntityTypeConfiguration<ClinicianResponse>
{
    public void Configure(EntityTypeBuilder<ClinicianResponse> builder)
    {
        builder.ToTable("clinician_response", DbSchemas.Clinical);
        builder.HasKey(response => response.Id);
        builder.Property(response => response.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(response => response.ConsultationId).HasColumnName("consultation_id");
        builder.Property(response => response.RecommendationKey).HasColumnName("recommendation_key").HasMaxLength(100).IsRequired();
        builder.Property(response => response.ResponseType).HasColumnName("response_type").HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(response => response.Rationale).HasColumnName("rationale").HasColumnType("text");
        builder.Property(response => response.ModifiedAction).HasColumnName("modified_action").HasColumnType("text");
        builder.Property(response => response.RespondedAt).HasColumnName("responded_at");
        builder.Property(response => response.ActorId).HasColumnName("actor_id").HasMaxLength(100).IsRequired();
        builder.Property(response => response.ActorRole).HasColumnName("actor_role").HasMaxLength(80).IsRequired();
        builder.HasIndex(response => new { response.ConsultationId, response.RecommendationKey, response.RespondedAt });
    }
}
