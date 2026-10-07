using ClarityClinical.Domain.Assessments;
using ClarityClinical.Domain.Consultations;

namespace ClarityClinical.Application.ClinicalIntelligence;

public sealed record ClinicalEvidenceResult(
    Guid FactId,
    string FactCode,
    string DisplayText,
    ClinicalFactSource Source);

public sealed record ClinicalAssessmentCandidateResult(
    string Key,
    string DisplayName,
    int CurrentScore,
    int? PreviousScore,
    PriorityScoreType ScoreType,
    IReadOnlyList<ClinicalEvidenceResult> SupportingEvidence,
    IReadOnlyList<string> MissingEvidence,
    IReadOnlyList<string> SuggestedChecks,
    IReadOnlyList<string> Warnings,
    string? ChangeReason);

public sealed record ClinicalIntelligenceResult(
    IReadOnlyList<ClinicalAssessmentCandidateResult> Candidates,
    IReadOnlyList<AssessmentChange> Changes,
    string RuleSetVersion);
