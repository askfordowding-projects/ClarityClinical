namespace ClarityClinical.Domain.Assessments;

public sealed class ClinicalAssessment
{
    private readonly List<AssessmentCandidate> _candidates = [];

    public ClinicalAssessment(Guid id, Guid consultationId, DateTimeOffset evaluatedAt)
    {
        Id = id;
        ConsultationId = consultationId;
        EvaluatedAt = evaluatedAt;
    }

    public Guid Id { get; }
    public Guid ConsultationId { get; }
    public DateTimeOffset EvaluatedAt { get; }
    public IReadOnlyCollection<AssessmentCandidate> Candidates => _candidates.AsReadOnly();

    public void AddCandidate(AssessmentCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        _candidates.Add(candidate);
    }
}
