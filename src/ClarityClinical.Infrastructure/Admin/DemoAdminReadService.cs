using ClarityClinical.Application.Admin;
using ClarityClinical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClarityClinical.Infrastructure.Admin;

public sealed class DemoAdminReadService(ClarityClinicalDbContext dbContext) : IDemoAdminReadService
{
    public async Task<IReadOnlyList<DemoPatientDto>> GetCanonicalPatientsAsync(CancellationToken cancellationToken)
    {
        var patientIds = dbContext.DemoScenarios
            .Where(scenario => scenario.IsCanonical)
            .Select(scenario => scenario.PatientId);

        return await dbContext.Patients
            .AsNoTracking()
            .Where(patient => patientIds.Contains(patient.Id))
            .OrderBy(patient => patient.FamilyName)
            .ThenBy(patient => patient.GivenName)
            .Select(patient => new DemoPatientDto(
                patient.Id,
                patient.GivenName + " " + patient.FamilyName,
                patient.DateOfBirth,
                true,
                patient.Conditions.Select(value => new DemoClinicalValueDto(value.Code, value.DisplayName)).ToArray(),
                patient.Allergies.Select(value => new DemoClinicalValueDto(value.Code, value.DisplayName)).ToArray(),
                patient.Medications.Select(value => new DemoClinicalValueDto(value.Code, value.DisplayName)).ToArray()))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DemoScenarioDto>> GetCanonicalScenariosAsync(CancellationToken cancellationToken) =>
        await dbContext.DemoScenarios
            .AsNoTracking()
            .Where(scenario => scenario.IsCanonical)
            .OrderBy(scenario => scenario.Name)
            .Select(scenario => new DemoScenarioDto(
                scenario.Id,
                scenario.ScenarioKey,
                scenario.Name,
                scenario.Setting,
                scenario.SourceLanguage,
                scenario.ClinicianLanguage,
                scenario.IsCanonical,
                scenario.Steps.Count))
            .ToArrayAsync(cancellationToken);
}
