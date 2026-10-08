using ClarityClinical.Application.ClinicalIntelligence;
using ClarityClinical.Application.Consultations.Responses;
using ClarityClinical.Application.Patients;
using ClarityClinical.Domain.Consultations;
using ClarityClinical.Application.Transcripts;

namespace ClarityClinical.Application.Consultations.Workspace;

public sealed class GetConsultationWorkspaceQuery(
    IConsultationRepository consultationRepository,
    IPatientRepository patientRepository,
    IClinicalIntelligenceProvider clinicalIntelligenceProvider,
    IClinicianResponseRepository responseRepository,
    ITranscriptSegmentRepository transcriptRepository)
{
    public async Task<ConsultationWorkspaceDto> ExecuteAsync(
        Guid consultationId,
        CancellationToken cancellationToken)
    {
        var consultation = await consultationRepository.GetAsync(consultationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Consultation '{consultationId}' was not found.");
        var patient = await patientRepository.GetAsync(consultation.PatientId, cancellationToken)
            ?? throw new KeyNotFoundException($"Patient '{consultation.PatientId}' was not found.");
        var intelligence = await clinicalIntelligenceProvider.EvaluateAsync(
            consultation,
            cancellationToken);

        var transcript = await transcriptRepository.GetByConsultationAsync(consultationId, cancellationToken);

        var responses = await responseRepository.GetLatestByConsultationAsync(
            consultationId,
            cancellationToken);
        var assessments = intelligence.Candidates
            .Select(candidate => ConsultationWorkspaceDto.MapCandidate(
                candidate,
                responses.TryGetValue(candidate.Key, out var response) ? response : null))
            .ToArray();
        var isInProgress = consultation.Status == ConsultationStatus.InProgress;

        return new ConsultationWorkspaceDto(
            new ConsultationSummaryDto(consultation.Id, consultation.Status.ToString()),
            new PatientContextDto(
                patient.Id,
                patient.DisplayName,
                CalculateAge(patient.DateOfBirth, DateOnly.FromDateTime(DateTime.UtcNow)),
                patient.Conditions.Select(value => new NamedClinicalValueDto(value.Code, value.DisplayName)).ToArray(),
                patient.Allergies.Select(value => new NamedClinicalValueDto(value.Code, value.DisplayName)).ToArray(),
                patient.Medications.Select(value => new NamedClinicalValueDto(value.Code, value.DisplayName)).ToArray()),
            transcript.Select(segment => new TranscriptEntryDto(
                segment.Id,
                segment.SpeakerRole.ToString(),
                segment.SourceLanguage,
                segment.IsRedacted ? "[Redacted]" : segment.DisplayOriginalText,
                segment.IsRedacted ? null : segment.TranslatedText,
                segment.RecognitionConfidence,
                segment.Corrected,
                segment.IncludeInReasoning,
                segment.IsRedacted)).ToArray(),
            assessments,
            assessments.SelectMany(value => value.Warnings).Distinct(StringComparer.Ordinal).ToArray(),
            assessments.SelectMany(value => value.SuggestedChecks).Distinct(StringComparer.Ordinal).ToArray(),
            new WorkspacePermissionsDto(isInProgress, isInProgress, isInProgress));
    }

    private static int CalculateAge(DateOnly dateOfBirth, DateOnly date)
    {
        var age = date.Year - dateOfBirth.Year;
        return date < dateOfBirth.AddYears(age) ? age - 1 : age;
    }
}
