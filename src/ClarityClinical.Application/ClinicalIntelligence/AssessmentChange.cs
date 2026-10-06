namespace ClarityClinical.Application.ClinicalIntelligence;

public sealed record AssessmentChange(
    string CandidateKey,
    int PreviousScore,
    int CurrentScore,
    string Reason);
