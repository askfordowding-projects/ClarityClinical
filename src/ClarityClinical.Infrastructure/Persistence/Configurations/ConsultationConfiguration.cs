using ClarityClinical.Domain.Consultations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClarityClinical.Infrastructure.Persistence.Configurations;

public sealed class ConsultationConfiguration : IEntityTypeConfiguration<Consultation>
{
    public void Configure(EntityTypeBuilder<Consultation> builder)
    {
        builder.ToTable("consultation", DbSchemas.Consultations);
        builder.HasKey(consultation => consultation.Id);

        builder.Property(consultation => consultation.Id).HasColumnName("id");
        builder.Property(consultation => consultation.PatientId).HasColumnName("patient_id");
        builder.Property(consultation => consultation.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32);
        builder.Property(consultation => consultation.StartedAt).HasColumnName("started_at");
        builder.Property(consultation => consultation.CompletedAt).HasColumnName("completed_at");

        builder.HasMany(consultation => consultation.Facts)
            .WithOne()
            .HasForeignKey("consultation_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(consultation => consultation.Facts).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
