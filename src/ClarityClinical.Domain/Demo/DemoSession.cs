namespace ClarityClinical.Domain.Demo;

public sealed class DemoSession
{
    public DemoSession(
        Guid id,
        Guid scenarioId,
        Guid consultationId,
        string visitorId,
        DateTimeOffset createdAt,
        DateTimeOffset lastActivityAt,
        DateTimeOffset expiresAt)
    {
        if (string.IsNullOrWhiteSpace(visitorId))
        {
            throw new ArgumentException("Visitor id is required.", nameof(visitorId));
        }

        Id = id;
        ScenarioId = scenarioId;
        ConsultationId = consultationId;
        VisitorId = visitorId;
        CreatedAt = createdAt;
        LastActivityAt = lastActivityAt;
        ExpiresAt = expiresAt;
    }

    public Guid Id { get; }
    public Guid ScenarioId { get; }
    public Guid ConsultationId { get; private set; }
    public string VisitorId { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset LastActivityAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }

    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;

    public void ReplaceConsultation(Guid consultationId, DateTimeOffset now, TimeSpan lifetime)
    {
        ConsultationId = consultationId;
        Touch(now, lifetime);
    }

    public void Touch(DateTimeOffset now, TimeSpan lifetime)
    {
        LastActivityAt = now;
        ExpiresAt = now.Add(lifetime);
    }
}
