using ClarityClinical.Application.Admin;
using ClarityClinical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClarityClinical.Infrastructure.Admin;

public sealed class AdminReadService(ClarityClinicalDbContext dbContext) : IAdminReadService
{
    public async Task<IReadOnlyList<GuidelineSourceDto>> GetGuidelinesAsync(
        CancellationToken cancellationToken) =>
        await dbContext.GuidelineSources
            .AsNoTracking()
            .OrderBy(source => source.ReferenceCode)
            .Select(source => new GuidelineSourceDto(
                source.ReferenceCode,
                source.Organisation,
                source.Title,
                source.Url,
                source.PublishedDate,
                source.LastUpdatedDate,
                source.LastReviewedDate,
                source.ReviewStatus))
            .ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyList<ClinicalRuleVersionDto>> GetRulesAsync(
        CancellationToken cancellationToken) =>
        await (
            from rule in dbContext.ClinicalRuleVersions.AsNoTracking()
            join source in dbContext.GuidelineSources.AsNoTracking()
                on rule.GuidelineSourceId equals source.Id
            where rule.Status == "Active"
            orderby rule.RuleKey
            select new ClinicalRuleVersionDto(
                rule.RuleKey,
                rule.DisplayName,
                rule.Version,
                rule.Status,
                rule.ScoreType,
                rule.IsIllustrative,
                rule.ProvenanceNote,
                rule.EffectiveFrom,
                new GuidelineSourceDto(
                    source.ReferenceCode,
                    source.Organisation,
                    source.Title,
                    source.Url,
                    source.PublishedDate,
                    source.LastUpdatedDate,
                    source.LastReviewedDate,
                    source.ReviewStatus)))
            .ToArrayAsync(cancellationToken);

    public async Task<AdminAuditPageDto> GetAuditAsync(
        string visitorId,
        Guid? consultationId,
        string? action,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var sessionIds = dbContext.DemoSessions
            .Where(session => session.VisitorId == visitorId)
            .Select(session => session.Id);
        var query = dbContext.AuditEvents
            .AsNoTracking()
            .Where(item => item.DemoSessionId != null && sessionIds.Contains(item.DemoSessionId.Value));

        if (consultationId is not null)
        {
            query = query.Where(item => item.ConsultationId == consultationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            var normalizedAction = action.Trim();
            query = query.Where(item => item.Action == normalizedAction);
        }

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(item => item.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new AdminAuditEventDto(
                item.Id,
                item.ConsultationId,
                item.ActorRole,
                item.Action,
                item.EntityType,
                item.EntityId,
                item.OccurredAt,
                item.CorrelationId,
                item.PreviousState,
                item.NewState,
                item.Reason,
                item.RuleVersion))
            .ToArrayAsync(cancellationToken);

        return new AdminAuditPageDto(totalCount, items);
    }
}
