using ClarityClinical.Domain.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClarityClinical.Infrastructure.Persistence.Configurations;

public sealed class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("audit_event", DbSchemas.Audit);
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(item => item.DemoSessionId).HasColumnName("demo_session_id");
        builder.Property(item => item.ConsultationId).HasColumnName("consultation_id");
        builder.Property(item => item.ActorId).HasColumnName("actor_id").HasMaxLength(100).IsRequired();
        builder.Property(item => item.ActorRole).HasColumnName("actor_role").HasMaxLength(80).IsRequired();
        builder.Property(item => item.Action).HasColumnName("action").HasMaxLength(100).IsRequired();
        builder.Property(item => item.EntityType).HasColumnName("entity_type").HasMaxLength(100).IsRequired();
        builder.Property(item => item.EntityId).HasColumnName("entity_id").HasMaxLength(200).IsRequired();
        builder.Property(item => item.OccurredAt).HasColumnName("occurred_at");
        builder.Property(item => item.CorrelationId).HasColumnName("correlation_id").HasMaxLength(64).IsRequired();
        builder.Property(item => item.PreviousState).HasColumnName("previous_state").HasColumnType("text");
        builder.Property(item => item.NewState).HasColumnName("new_state").HasColumnType("text");
        builder.Property(item => item.Reason).HasColumnName("reason").HasColumnType("text");
        builder.Property(item => item.RuleVersion).HasColumnName("rule_version").HasMaxLength(100);
        builder.HasIndex(item => new { item.ConsultationId, item.OccurredAt });
        builder.HasIndex(item => item.CorrelationId);
    }
}
