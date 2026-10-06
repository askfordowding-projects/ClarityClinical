using ClarityClinical.Application.Identity;

namespace ClarityClinical.Infrastructure.Identity;

public sealed class DemoIdentityProvider : IDemoIdentityProvider
{
    public CurrentUser? GetDemoUser(string role)
    {
        return role switch
        {
            "Clinician" => new CurrentUser(
                "demo-clinician",
                "Demo Clinician",
                "Clinician",
                true),
            "Administrator" => new CurrentUser(
                "demo-administrator",
                "Demo Administrator",
                "Administrator",
                true),
            _ => null
        };
    }
}
