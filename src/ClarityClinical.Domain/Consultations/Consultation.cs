namespace ClarityClinical.Domain.Consultations;

public sealed class Consultation
{
    private readonly List<ClinicalFact> _facts = [];

    public Consultation(Guid id, Guid patientId)
    {
        Id = id;
        PatientId = patientId;
        Status = ConsultationStatus.NotStarted;
    }

    public Guid Id { get; }
    public Guid PatientId { get; }
    public ConsultationStatus Status { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public IReadOnlyCollection<ClinicalFact> Facts => _facts.AsReadOnly();

    public void Start(DateTimeOffset now)
    {
        if (Status != ConsultationStatus.NotStarted)
        {
            throw new InvalidOperationException("Only a not started consultation can be started.");
        }

        Status = ConsultationStatus.InProgress;
        StartedAt = now;
    }

    public void Complete(DateTimeOffset now)
    {
        if (Status != ConsultationStatus.InProgress)
        {
            throw new InvalidOperationException("Only an in progress consultation can be completed.");
        }

        Status = ConsultationStatus.Completed;
        CompletedAt = now;
    }

    public void AddFact(ClinicalFact fact)
    {
        ArgumentNullException.ThrowIfNull(fact);
        _facts.Add(fact);
    }

    public void SynchronizeFactsFromEvent(
        Guid sourceEventId,
        IEnumerable<ClinicalFact> desiredFacts)
    {
        ArgumentNullException.ThrowIfNull(desiredFacts);
        var desired = desiredFacts.ToArray();
        if (desired.Any(fact => fact.SourceEventId != sourceEventId))
        {
            throw new InvalidOperationException(
                "Projected clinical facts must reference the source event being synchronized.");
        }

        var desiredKeys = desired
            .Select(FactKey)
            .ToHashSet();
        var existing = _facts
            .Where(fact => fact.SourceEventId == sourceEventId)
            .ToArray();

        foreach (var fact in existing)
        {
            if (desiredKeys.Contains(FactKey(fact)))
            {
                fact.RestoreToReasoning();
            }
            else
            {
                fact.ExcludeFromReasoning();
            }
        }

        var existingKeys = existing
            .Select(FactKey)
            .ToHashSet();
        foreach (var fact in desired.Where(fact => !existingKeys.Contains(FactKey(fact))))
        {
            _facts.Add(fact);
        }
    }

    private static (string Code, ClinicalFactSource Source) FactKey(ClinicalFact fact) =>
        (fact.Code, fact.Source);
}
