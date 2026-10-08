namespace ClarityClinical.Application.Admin;

public sealed record GuidelineSourceDto(
    string ReferenceCode,
    string Organisation,
    string Title,
    string? Url,
    DateOnly PublishedDate,
    DateOnly? LastUpdatedDate,
    DateOnly? LastReviewedDate,
    string ReviewStatus);

public sealed record ClinicalRuleVersionDto(
    string RuleKey,
    string DisplayName,
    string Version,
    string Status,
    string ScoreType,
    bool IsIllustrative,
    string ProvenanceNote,
    DateOnly EffectiveFrom,
    GuidelineSourceDto Source);

public sealed record AdminAuditEventDto(
    Guid Id,
    Guid ConsultationId,
    string ActorRole,
    string Action,
    string EntityType,
    string EntityId,
    DateTimeOffset OccurredAt,
    string CorrelationId,
    string? PreviousState,
    string? NewState,
    string? Reason,
    string? RuleVersion);

public sealed record AdminAuditPageDto(
    int TotalCount,
    IReadOnlyList<AdminAuditEventDto> Items);

public interface IAdminReadService
{
    Task<IReadOnlyList<GuidelineSourceDto>> GetGuidelinesAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ClinicalRuleVersionDto>> GetRulesAsync(
        CancellationToken cancellationToken);

    Task<AdminAuditPageDto> GetAuditAsync(
        string visitorId,
        Guid? consultationId,
        string? action,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
