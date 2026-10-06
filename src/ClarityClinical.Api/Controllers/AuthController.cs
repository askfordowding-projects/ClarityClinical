using System.Security.Claims;
using ClarityClinical.Application.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClarityClinical.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IDemoIdentityProvider demoIdentityProvider) : ControllerBase
{
    [HttpPost("demo-login")]
    [AllowAnonymous]
    public async Task<IActionResult> DemoLogin(
        DemoLoginRequest request,
        CancellationToken cancellationToken)
    {
        var demoUser = demoIdentityProvider.GetDemoUser(request.Role);
        if (demoUser is null)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Unknown demo role",
                detail: "Choose either Clinician or Administrator.");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, demoUser.UserId),
            new Claim(ClaimTypes.Name, demoUser.DisplayName),
            new Claim(ClaimTypes.Role, demoUser.Role),
            new Claim("is_demo", bool.TrueString)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme));

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = false });

        return Ok(new CurrentUserResponse(
            demoUser.DisplayName,
            demoUser.Role,
            demoUser.IsDemo));
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var isDemo = bool.TryParse(User.FindFirstValue("is_demo"), out var value) && value;
        return Ok(new CurrentUserResponse(User.Identity?.Name ?? string.Empty, role, isDemo));
    }

    public sealed record DemoLoginRequest(string Role);
    public sealed record CurrentUserResponse(string DisplayName, string Role, bool IsDemo);
}
