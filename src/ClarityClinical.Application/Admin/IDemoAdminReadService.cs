namespace ClarityClinical.Application.Admin;

public sealed record DemoClinicalValueDto(string Code, string DisplayName);

public sealed record DemoPatientDto(
    Guid Id,
    string DisplayName,
    DateOnly DateOfBirth,
    bool IsCanonical,
    IReadOnlyList<DemoClinicalValueDto> Conditions,
    IReadOnlyList<DemoClinicalValueDto> Allergies,
    IReadOnlyList<DemoClinicalValueDto> Medications);

public sealed record DemoScenarioDto(
    Guid Id,
    string ScenarioKey,
    string Name,
    string Setting,
    string SourceLanguage,
    string ClinicianLanguage,
    bool IsCanonical,
    int StepCount);

public interface IDemoAdminReadService
{
    Task<IReadOnlyList<DemoPatientDto>> GetCanonicalPatientsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<DemoScenarioDto>> GetCanonicalScenariosAsync(CancellationToken cancellationToken);
}
