namespace ClarityClinical.Application.Demo;

public sealed record DemoSandboxValueDto(string Code, string DisplayName);
public sealed record DemoSandboxPatientUpdate(
    string GivenName,
    string FamilyName,
    DateOnly DateOfBirth,
    IReadOnlyList<DemoSandboxValueDto> Conditions,
    IReadOnlyList<DemoSandboxValueDto> Allergies,
    IReadOnlyList<DemoSandboxValueDto> Medications);
public sealed record DemoSandboxScenarioUpdate(
    string Name,
    string Description,
    string Setting,
    string SourceLanguage,
    string ClinicianLanguage);
public sealed record DemoSandboxPatientDto(
    Guid Id, string GivenName, string FamilyName, string DisplayName, DateOnly DateOfBirth,
    IReadOnlyList<DemoSandboxValueDto> Conditions,
    IReadOnlyList<DemoSandboxValueDto> Allergies,
    IReadOnlyList<DemoSandboxValueDto> Medications);
public sealed record DemoSandboxScenarioDto(
    Guid Id, string ScenarioKey, string Name, string Description, string Setting,
    string SourceLanguage, string ClinicianLanguage, bool IsCanonical, int StepCount);
public sealed record DemoSandboxSnapshotDto(
    Guid Id, Guid SourceScenarioId, DemoSandboxPatientDto Patient, DemoSandboxScenarioDto Scenario);

public interface IDemoSandboxService
{
    Task<DemoSandboxSnapshotDto> GetOrCreateAsync(string scenarioKey, string visitorId, CancellationToken cancellationToken);
    Task<DemoSandboxSnapshotDto> UpdatePatientAsync(string scenarioKey, string visitorId, DemoSandboxPatientUpdate update, CancellationToken cancellationToken);
    Task<DemoSandboxSnapshotDto> UpdateScenarioAsync(string scenarioKey, string visitorId, DemoSandboxScenarioUpdate update, CancellationToken cancellationToken);
    Task<int> DeleteExpiredAsync(DateTimeOffset now, TimeSpan inactivityLifetime, CancellationToken cancellationToken);
}
