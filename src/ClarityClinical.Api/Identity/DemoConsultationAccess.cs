using System.Security.Claims;
using ClarityClinical.Application.Demo;

namespace ClarityClinical.Api.Identity;

public sealed class DemoConsultationAccess(
    IHttpContextAccessor httpContextAccessor,
    IDemoSessionService demoSessionService)
{
    public string? VisitorId =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(DemoClaimTypes.VisitorId);

    public async Task<bool> CanAccessAsync(
        Guid consultationId,
        CancellationToken cancellationToken)
    {
        var principal = httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var isDemo = bool.TryParse(
            principal.FindFirstValue(DemoClaimTypes.IsDemo),
            out var parsed) && parsed;
        if (!isDemo)
        {
            return true;
        }

        var visitorId = VisitorId;
        return !string.IsNullOrWhiteSpace(visitorId)
            && await demoSessionService.OwnsConsultationAsync(
                consultationId,
                visitorId,
                cancellationToken);
    }
}
