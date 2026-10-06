using ClarityClinical.Domain.Patients;

namespace ClarityClinical.Domain.Tests.Patients;

public sealed class PatientTests
{
    [Fact]
    public void Patient_exposes_display_name_and_date_of_birth()
    {
        var patient = new Patient(
            Guid.NewGuid(),
            "Miguel",
            "Santos",
            new DateOnly(1968, 6, 15));

        Assert.Equal("Miguel Santos", patient.DisplayName);
        Assert.Equal(new DateOnly(1968, 6, 15), patient.DateOfBirth);
    }

    [Fact]
    public void Patient_can_add_condition_allergy_and_medication()
    {
        var patient = new Patient(
            Guid.NewGuid(),
            "Miguel",
            "Santos",
            new DateOnly(1968, 6, 15));

        patient.AddCondition(new PatientCondition("type-2-diabetes", "Type 2 diabetes"));
        patient.AddAllergy(new PatientAllergy("penicillin", "Penicillin"));
        patient.AddMedication(new PatientMedication("metformin", "Metformin"));

        Assert.Single(patient.Conditions);
        Assert.Single(patient.Allergies);
        Assert.Single(patient.Medications);
    }
}
