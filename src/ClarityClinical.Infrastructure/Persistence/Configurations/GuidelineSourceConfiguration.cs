using ClarityClinical.Domain.Governance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClarityClinical.Infrastructure.Persistence.Configurations;

public sealed class GuidelineSourceConfiguration : IEntityTypeConfiguration<GuidelineSource>
{
    public void Configure(EntityTypeBuilder<GuidelineSource> builder)
    {
        builder.ToTable("guideline_source", DbSchemas.Governance);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.ReferenceCode).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => x.ReferenceCode).IsUnique();
        builder.Property(x => x.Organisation).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(512).IsRequired();
        builder.Property(x => x.Url).HasMaxLength(1024);
        builder.Property(x => x.ReviewStatus).HasMaxLength(64).IsRequired();
    }
}
