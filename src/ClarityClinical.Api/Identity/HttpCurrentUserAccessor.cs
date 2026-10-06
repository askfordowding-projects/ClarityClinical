using System.Security.Claims;
using ClarityClinical.Application.Identity;

namespace ClarityClinical.Api.Identity;

public sealed class HttpCurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserAccessor
{
    public CurrentUser? CurrentUser
    {
        get
        {
            var principal = httpContextAccessor.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var displayName = principal.Identity.Name ?? string.Empty;
            var role = principal.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
            var isDemo = bool.TryParse(principal.FindFirstValue(DemoClaimTypes.IsDemo), out var parsed) && parsed;
            return new CurrentUser(userId, displayName, role, isDemo);
        }
    }
}
