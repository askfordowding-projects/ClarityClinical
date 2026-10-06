using ClarityClinical.Application.ClinicalIntelligence;

namespace ClarityClinical.Application.Consultations.Workspace;

public sealed record ConsultationSummaryDto(Guid Id, string Status);

public sealed record NamedClinicalValueDto(string Code, string DisplayName);

public sealed record PatientContextDto(
    Guid Id,
    string DisplayName,
    int Age,
    IReadOnlyList<NamedClinicalValueDto> Conditions,
    IReadOnlyList<NamedClinicalValueDto> Allergies,
    IReadOnlyList<NamedClinicalValueDto> Medications);

public sealed record TranscriptEntryDto(
    Guid Id,
    string SpeakerRole,
    string OriginalLanguage,
    string OriginalText,
    string? TranslatedText,
    bool Corrected);

public sealed record EvidenceDto(
    string FactCode,
    string DisplayText,
    string Source);

public sealed record AssessmentCandidateDto(
    string Key,
    string DisplayName,
    int CurrentScore,
    int? PreviousScore,
    string ScoreType,
    IReadOnlyList<EvidenceDto> SupportingEvidence,
    IReadOnlyList<string> MissingEvidence,
    IReadOnlyList<string> SuggestedChecks,
    IReadOnlyList<string> Warnings,
    string? ChangeReason);

public sealed record WorkspacePermissionsDto(
    bool CanRecordObservation,
    bool CanAddPatientReport,
    bool CanCompleteConsultation);

public sealed record ConsultationWorkspaceDto(
    ConsultationSummaryDto Consultation,
    PatientContextDto Patient,
    IReadOnlyList<TranscriptEntryDto> Transcript,
    IReadOnlyList<AssessmentCandidateDto> Assessments,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> SuggestedChecks,
    WorkspacePermissionsDto Permissions)
{
    public static AssessmentCandidateDto MapCandidate(ClinicalAssessmentCandidateResult candidate) =>
        new(
            candidate.Key,
            candidate.DisplayName,
            candidate.CurrentScore,
            candidate.PreviousScore,
            candidate.ScoreType.ToString(),
            candidate.SupportingEvidence
                .Select(evidence => new EvidenceDto(
                    evidence.FactCode,
                    evidence.DisplayText,
                    evidence.Source.ToString()))
                .ToArray(),
            candidate.MissingEvidence,
            candidate.SuggestedChecks,
            candidate.Warnings,
            candidate.ChangeReason);
}
