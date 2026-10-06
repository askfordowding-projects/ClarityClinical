using ClarityClinical.Domain.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClarityClinical.Infrastructure.Persistence.Configurations;

public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("patient", DbSchemas.Patients);
        builder.HasKey(patient => patient.Id);

        builder.Property(patient => patient.Id).HasColumnName("id");
        builder.Property(patient => patient.GivenName).HasColumnName("given_name").HasMaxLength(100).IsRequired();
        builder.Property(patient => patient.FamilyName).HasColumnName("family_name").HasMaxLength(100).IsRequired();
        builder.Property(patient => patient.DateOfBirth).HasColumnName("date_of_birth").IsRequired();
        builder.Ignore(patient => patient.DisplayName);

        builder.OwnsMany(patient => patient.Conditions, owned =>
        {
            owned.ToTable("patient_condition", DbSchemas.Patients);
            owned.WithOwner().HasForeignKey("patient_id");
            owned.Property(condition => condition.Code).HasColumnName("code").HasMaxLength(100);
            owned.Property(condition => condition.DisplayName).HasColumnName("display_name").HasMaxLength(200);
            owned.HasKey("patient_id", nameof(PatientCondition.Code));
        });

        builder.OwnsMany(patient => patient.Allergies, owned =>
        {
            owned.ToTable("patient_allergy", DbSchemas.Patients);
            owned.WithOwner().HasForeignKey("patient_id");
            owned.Property(allergy => allergy.Code).HasColumnName("code").HasMaxLength(100);
            owned.Property(allergy => allergy.DisplayName).HasColumnName("display_name").HasMaxLength(200);
            owned.HasKey("patient_id", nameof(PatientAllergy.Code));
        });

        builder.OwnsMany(patient => patient.Medications, owned =>
        {
            owned.ToTable("patient_medication", DbSchemas.Patients);
            owned.WithOwner().HasForeignKey("patient_id");
            owned.Property(medication => medication.Code).HasColumnName("code").HasMaxLength(100);
            owned.Property(medication => medication.DisplayName).HasColumnName("display_name").HasMaxLength(200);
            owned.HasKey("patient_id", nameof(PatientMedication.Code));
        });

        builder.Navigation(patient => patient.Conditions).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(patient => patient.Allergies).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(patient => patient.Medications).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
