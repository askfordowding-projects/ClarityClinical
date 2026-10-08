using ClarityClinical.Domain.Governance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClarityClinical.Infrastructure.Persistence.Configurations;

public sealed class ClinicalRuleVersionConfiguration : IEntityTypeConfiguration<ClinicalRuleVersion>
{
    public void Configure(EntityTypeBuilder<ClinicalRuleVersion> builder)
    {
        builder.ToTable("clinical_rule_version", DbSchemas.Governance);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.RuleKey).HasMaxLength(128).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Version).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.ScoreType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ProvenanceNote).HasMaxLength(2048).IsRequired();
        builder.HasIndex(x => new { x.RuleKey, x.Version }).IsUnique();
        builder.HasOne<GuidelineSource>()
            .WithMany()
            .HasForeignKey(x => x.GuidelineSourceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
