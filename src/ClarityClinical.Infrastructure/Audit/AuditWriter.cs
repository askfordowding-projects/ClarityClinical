using ClarityClinical.Application.Audit;
using ClarityClinical.Application.Identity;
using ClarityClinical.Domain.Audit;
using ClarityClinical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClarityClinical.Infrastructure.Audit;

public sealed class AuditWriter(
    ClarityClinicalDbContext dbContext,
    ICurrentUserAccessor currentUserAccessor) : IAuditWriter
{
    public async Task WriteAsync(
        AuditWriteRequest request,
        CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.CurrentUser
            ?? new CurrentUser("system", "System", "System", true);
        var demoSessionId = await dbContext.DemoSessions
            .Where(session => session.ConsultationId == request.ConsultationId)
            .Select(session => (Guid?)session.Id)
            .SingleOrDefaultAsync(cancellationToken);

        dbContext.AuditEvents.Add(new AuditEvent(
            Guid.NewGuid(),
            demoSessionId,
            request.ConsultationId,
            currentUser.UserId,
            currentUser.Role,
            request.Action,
            request.EntityType,
            request.EntityId,
            DateTimeOffset.UtcNow,
            request.CorrelationId,
            request.PreviousState,
            request.NewState,
            request.Reason,
            request.RuleVersion));
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
