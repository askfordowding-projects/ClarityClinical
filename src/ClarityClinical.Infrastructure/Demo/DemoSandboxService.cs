using ClarityClinical.Application.Demo;
using ClarityClinical.Domain.Demo;
using ClarityClinical.Domain.Patients;
using ClarityClinical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClarityClinical.Infrastructure.Demo;

public sealed class DemoSandboxService(ClarityClinicalDbContext dbContext) : IDemoSandboxService
{
    public async Task<DemoSandboxSnapshotDto> GetOrCreateAsync(string scenarioKey, string visitorId, CancellationToken cancellationToken)
    {
        var source = await dbContext.DemoScenarios
            .Include(x => x.Steps)
            .SingleOrDefaultAsync(x => x.ScenarioKey == scenarioKey && x.IsCanonical, cancellationToken)
            ?? throw new KeyNotFoundException($"Demo scenario '{scenarioKey}' was not found.");

        var existing = await dbContext.DemoSandboxes
            .SingleOrDefaultAsync(x => x.VisitorId == visitorId && x.SourceScenarioId == source.Id, cancellationToken);
        if (existing is not null) return await BuildSnapshotAsync(existing, cancellationToken);

        var sourcePatient = await dbContext.Patients
            .Include(x => x.Conditions).Include(x => x.Allergies).Include(x => x.Medications)
            .SingleAsync(x => x.Id == source.PatientId, cancellationToken);

        var patient = new Patient(Guid.NewGuid(), sourcePatient.GivenName, sourcePatient.FamilyName, sourcePatient.DateOfBirth);
        foreach (var value in sourcePatient.Conditions) patient.AddCondition(new PatientCondition(value.Code, value.DisplayName));
        foreach (var value in sourcePatient.Allergies) patient.AddAllergy(new PatientAllergy(value.Code, value.DisplayName));
        foreach (var value in sourcePatient.Medications) patient.AddMedication(new PatientMedication(value.Code, value.DisplayName));

        var sandboxId = Guid.NewGuid();
        var scenarioId = Guid.NewGuid();
        var scenario = new DemoScenario(
            scenarioId,
            $"{source.ScenarioKey}-sandbox-{sandboxId:N}",
            source.Name, source.Description, patient.Id, source.Setting, source.SourceLanguage, source.ClinicianLanguage, false);
        foreach (var step in source.Steps.OrderBy(x => x.Sequence))
        {
            scenario.AddStep(new DemoScenarioStep(
                Guid.NewGuid(), scenario.Id, step.Sequence, step.StepType,
                step.ClinicalFactCode, step.ClinicalFactDisplayText, step.OriginalText, step.SourceLanguage));
        }

        var now = DateTimeOffset.UtcNow;
        var sandbox = new DemoSandbox(sandboxId, visitorId, source.Id, scenario.Id, patient.Id, now);
        dbContext.Patients.Add(patient);
        dbContext.DemoScenarios.Add(scenario);
        dbContext.DemoSandboxes.Add(sandbox);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await BuildSnapshotAsync(sandbox, cancellationToken);
    }

    public async Task<DemoSandboxSnapshotDto> UpdatePatientAsync(string scenarioKey, string visitorId, DemoSandboxPatientUpdate update, CancellationToken cancellationToken)
    {
        var sandbox = await GetSandboxEntityAsync(scenarioKey, visitorId, cancellationToken) ??
            await CreateAndReloadAsync(scenarioKey, visitorId, cancellationToken);
        var patient = await dbContext.Patients
            .Include(x => x.Conditions).Include(x => x.Allergies).Include(x => x.Medications)
            .SingleAsync(x => x.Id == sandbox.SandboxPatientId, cancellationToken);
        ArgumentNullException.ThrowIfNull(update.Conditions);
        ArgumentNullException.ThrowIfNull(update.Allergies);
        ArgumentNullException.ThrowIfNull(update.Medications);
        patient.UpdateDemographics(update.GivenName, update.FamilyName, update.DateOfBirth);
        patient.ReplaceConditions(update.Conditions.Select(x => new PatientCondition(x.Code, x.DisplayName)));
        patient.ReplaceAllergies(update.Allergies.Select(x => new PatientAllergy(x.Code, x.DisplayName)));
        patient.ReplaceMedications(update.Medications.Select(x => new PatientMedication(x.Code, x.DisplayName)));
        sandbox.Touch(DateTimeOffset.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await BuildSnapshotAsync(sandbox, cancellationToken);
    }

    public async Task<DemoSandboxSnapshotDto> UpdateScenarioAsync(string scenarioKey, string visitorId, DemoSandboxScenarioUpdate update, CancellationToken cancellationToken)
    {
        var sandbox = await GetSandboxEntityAsync(scenarioKey, visitorId, cancellationToken) ??
            await CreateAndReloadAsync(scenarioKey, visitorId, cancellationToken);
        var scenario = await dbContext.DemoScenarios
            .SingleAsync(x => x.Id == sandbox.SandboxScenarioId, cancellationToken);
        scenario.UpdateDetails(update.Name, update.Description, update.Setting, update.SourceLanguage, update.ClinicianLanguage);
        sandbox.Touch(DateTimeOffset.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await BuildSnapshotAsync(sandbox, cancellationToken);
    }

    public async Task<int> DeleteExpiredAsync(DateTimeOffset now, TimeSpan inactivityLifetime, CancellationToken cancellationToken)
    {
        var cutoff = now.Subtract(inactivityLifetime);
        var expired = await dbContext.DemoSandboxes
            .Where(sandbox => sandbox.UpdatedAt <= cutoff
                && !dbContext.DemoSessions.Any(session =>
                    session.ScenarioId == sandbox.SandboxScenarioId && session.ExpiresAt > now))
            .Select(sandbox => new { sandbox.Id, sandbox.SandboxScenarioId, sandbox.SandboxPatientId })
            .ToArrayAsync(cancellationToken);
        if (expired.Length == 0) return 0;

        var sandboxIds = expired.Select(x => x.Id).ToArray();
        var scenarioIds = expired.Select(x => x.SandboxScenarioId).ToArray();
        var patientIds = expired.Select(x => x.SandboxPatientId).ToArray();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await dbContext.DemoSandboxes.Where(x => sandboxIds.Contains(x.Id)).ExecuteDeleteAsync(cancellationToken);
        await dbContext.DemoScenarios.Where(x => scenarioIds.Contains(x.Id)).ExecuteDeleteAsync(cancellationToken);
        await dbContext.Patients.Where(x => patientIds.Contains(x.Id)).ExecuteDeleteAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return expired.Length;
    }

    private async Task<DemoSandbox> CreateAndReloadAsync(string scenarioKey, string visitorId, CancellationToken cancellationToken)
    {
        await GetOrCreateAsync(scenarioKey, visitorId, cancellationToken);
        return await GetSandboxEntityAsync(scenarioKey, visitorId, cancellationToken)
            ?? throw new InvalidOperationException("Sandbox creation did not persist.");
    }

    private async Task<DemoSandbox?> GetSandboxEntityAsync(string scenarioKey, string visitorId, CancellationToken cancellationToken)
    {
        var sourceId = await dbContext.DemoScenarios
            .Where(x => x.ScenarioKey == scenarioKey && x.IsCanonical)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(cancellationToken);
        return sourceId is null ? null : await dbContext.DemoSandboxes
            .SingleOrDefaultAsync(x => x.VisitorId == visitorId && x.SourceScenarioId == sourceId.Value, cancellationToken);
    }

    private async Task<DemoSandboxSnapshotDto> BuildSnapshotAsync(DemoSandbox sandbox, CancellationToken cancellationToken)
    {
        var patient = await dbContext.Patients.AsNoTracking()
            .Include(x => x.Conditions).Include(x => x.Allergies).Include(x => x.Medications)
            .SingleAsync(x => x.Id == sandbox.SandboxPatientId, cancellationToken);
        var scenario = await dbContext.DemoScenarios.AsNoTracking().Include(x => x.Steps)
            .SingleAsync(x => x.Id == sandbox.SandboxScenarioId, cancellationToken);
        return new DemoSandboxSnapshotDto(
            sandbox.Id, sandbox.SourceScenarioId,
            new DemoSandboxPatientDto(patient.Id, patient.GivenName, patient.FamilyName, patient.DisplayName, patient.DateOfBirth,
                patient.Conditions.Select(x => new DemoSandboxValueDto(x.Code, x.DisplayName)).ToArray(),
                patient.Allergies.Select(x => new DemoSandboxValueDto(x.Code, x.DisplayName)).ToArray(),
                patient.Medications.Select(x => new DemoSandboxValueDto(x.Code, x.DisplayName)).ToArray()),
            new DemoSandboxScenarioDto(scenario.Id, scenario.ScenarioKey, scenario.Name, scenario.Description, scenario.Setting, scenario.SourceLanguage, scenario.ClinicianLanguage, scenario.IsCanonical, scenario.Steps.Count));
    }
}
