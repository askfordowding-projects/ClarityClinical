using ClarityClinical.Domain.Demo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClarityClinical.Infrastructure.Persistence.Configurations;

public sealed class DemoSessionConfiguration : IEntityTypeConfiguration<DemoSession>
{
    public void Configure(EntityTypeBuilder<DemoSession> builder)
    {
        builder.ToTable("demo_session", DbSchemas.Admin);
        builder.HasKey(session => session.Id);
        builder.Property(session => session.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(session => session.ScenarioId).HasColumnName("scenario_id");
        builder.Property(session => session.ConsultationId).HasColumnName("consultation_id");
        builder.Property(session => session.VisitorId).HasColumnName("visitor_id").HasMaxLength(100).IsRequired();
        builder.Property(session => session.CreatedAt).HasColumnName("created_at");
        builder.Property(session => session.LastActivityAt).HasColumnName("last_activity_at");
        builder.Property(session => session.ExpiresAt).HasColumnName("expires_at");
        builder.HasIndex(session => new { session.VisitorId, session.ExpiresAt });
        builder.HasIndex(session => session.ConsultationId).IsUnique();
    }
}
