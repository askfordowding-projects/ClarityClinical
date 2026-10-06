namespace ClarityClinical.Application.Identity;

public sealed record CurrentUser(
    string UserId,
    string DisplayName,
    string Role,
    bool IsDemo);
