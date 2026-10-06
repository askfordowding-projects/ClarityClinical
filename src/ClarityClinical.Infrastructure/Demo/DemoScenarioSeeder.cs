using ClarityClinical.Domain.Demo;
using ClarityClinical.Domain.Patients;
using ClarityClinical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClarityClinical.Infrastructure.Demo;

public sealed class DemoScenarioSeeder(ClarityClinicalDbContext dbContext)
{
    public const string MiguelScenarioKey = "miguel-santos-leg-swelling";

    private static readonly Guid MiguelPatientId = Guid.Parse("c11a760d-51f3-4ffd-8d5e-a642e96bed11");
    private static readonly Guid MiguelScenarioId = Guid.Parse("da4162fe-dba2-478d-8b0a-29be885e1491");

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.DemoScenarios.AnyAsync(
                scenario => scenario.ScenarioKey == MiguelScenarioKey,
                cancellationToken))
        {
            return;
        }

        var patient = await dbContext.Patients.FindAsync([MiguelPatientId], cancellationToken);
        if (patient is null)
        {
            patient = CreateMiguelPatient();
            dbContext.Patients.Add(patient);
        }

        var scenario = new DemoScenario(
            MiguelScenarioId,
            MiguelScenarioKey,
            "Miguel Santos - calf swelling",
            "Synthetic urgent-treatment-centre consultation demonstrating competing DVT, cellulitis and musculoskeletal assessments.",
            patient.Id,
            "Urgent Treatment Centre",
            "es",
            "en",
            true);

        scenario.AddStep(new DemoScenarioStep(
            Guid.Parse("cff60323-0e5a-42f1-9bad-c4d604f8a59c"),
            scenario.Id,
            1,
            DemoScenarioStepType.PatientSpeech,
            "unilateral-calf-swelling",
            "Unilateral calf swelling",
            "Tengo la pierna izquierda hinchada desde ayer.",
            "es"));

        scenario.AddStep(new DemoScenarioStep(
            Guid.Parse("b2ba55c8-a2f8-4898-85c3-cf58c63f9866"),
            scenario.Id,
            2,
            DemoScenarioStepType.PatientSpeech,
            "recent-prolonged-travel",
            "Recent prolonged travel",
            "Volví de España hace dos días. El vuelo duró ocho horas.",
            "es"));

        scenario.AddStep(new DemoScenarioStep(
            Guid.Parse("821a3edc-6dd6-43fe-820e-4db248f51f59"),
            scenario.Id,
            3,
            DemoScenarioStepType.ClinicalObservation,
            "calf-asymmetry",
            "Unilateral calf asymmetry"));

        scenario.AddStep(new DemoScenarioStep(
            Guid.Parse("9324597b-04ea-4440-879a-685e2e7ddcb8"),
            scenario.Id,
            4,
            DemoScenarioStepType.ClinicalObservation,
            "ankle-wound-warmth",
            "Ankle wound with local warmth"));

        dbContext.DemoScenarios.Add(scenario);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Patient CreateMiguelPatient()
    {
        var patient = new Patient(
            MiguelPatientId,
            "Miguel",
            "Santos",
            new DateOnly(1968, 6, 15));

        patient.AddCondition(new PatientCondition("type-2-diabetes", "Type 2 diabetes"));
        patient.AddCondition(new PatientCondition("hypertension", "Hypertension"));
        patient.AddCondition(new PatientCondition("asthma", "Asthma"));

        patient.AddAllergy(new PatientAllergy("penicillin", "Penicillin"));
        patient.AddAllergy(new PatientAllergy("ibuprofen", "Ibuprofen"));

        patient.AddMedication(new PatientMedication("metformin", "Metformin"));
        patient.AddMedication(new PatientMedication("ramipril", "Ramipril"));
        patient.AddMedication(new PatientMedication("paracetamol", "Paracetamol"));
        patient.AddMedication(new PatientMedication("salbutamol", "Salbutamol"));

        return patient;
    }
}
