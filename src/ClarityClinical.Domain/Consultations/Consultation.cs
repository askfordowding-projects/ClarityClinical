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
}
