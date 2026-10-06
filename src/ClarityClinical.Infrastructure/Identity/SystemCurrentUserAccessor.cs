using ClarityClinical.Application.Identity;

namespace ClarityClinical.Infrastructure.Identity;

public sealed class SystemCurrentUserAccessor : ICurrentUserAccessor
{
    private static readonly CurrentUser SystemUser = new(
        "system",
        "System",
        "System",
        true);

    public CurrentUser? CurrentUser => SystemUser;
}
