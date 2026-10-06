namespace ClarityClinical.Application.Identity;

public interface ICurrentUserAccessor
{
    CurrentUser? CurrentUser { get; }
}
