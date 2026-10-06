namespace ClarityClinical.Application.Identity;

public interface IDemoIdentityProvider
{
    CurrentUser? GetDemoUser(string role);
}
