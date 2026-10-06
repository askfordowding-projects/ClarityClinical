namespace ClarityClinical.Domain.Audit;

public sealed class AuditEvent
{
    public AuditEvent(
        Guid id,
        Guid? demoSessionId,
        Guid consultationId,
        string actorId,
        string actorRole,
        string action,
        string entityType,
        string entityId,
        DateTimeOffset occurredAt,
        string correlationId,
        string? previousState = null,
        string? newState = null,
        string? reason = null,
        string? ruleVersion = null)
    {
        Id = id;
        DemoSessionId = demoSessionId;
        ConsultationId = consultationId;
        ActorId = actorId;
        ActorRole = actorRole;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        OccurredAt = occurredAt;
        CorrelationId = correlationId;
        PreviousState = previousState;
        NewState = newState;
        Reason = reason;
        RuleVersion = ruleVersion;
    }

    public Guid Id { get; }
    public Guid? DemoSessionId { get; }
    public Guid ConsultationId { get; }
    public string ActorId { get; }
    public string ActorRole { get; }
    public string Action { get; }
    public string EntityType { get; }
    public string EntityId { get; }
    public DateTimeOffset OccurredAt { get; }
    public string CorrelationId { get; }
    public string? PreviousState { get; }
    public string? NewState { get; }
    public string? Reason { get; }
    public string? RuleVersion { get; }
}
