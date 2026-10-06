namespace ClarityClinical.Application.Audit;

public sealed record AuditWriteRequest(
    Guid ConsultationId,
    string Action,
    string EntityType,
    string EntityId,
    string CorrelationId,
    string? PreviousState = null,
    string? NewState = null,
    string? Reason = null,
    string? RuleVersion = null);

public interface IAuditWriter
{
    Task WriteAsync(
        AuditWriteRequest request,
        CancellationToken cancellationToken);
}
