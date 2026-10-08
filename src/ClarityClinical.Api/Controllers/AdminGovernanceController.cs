using ClarityClinical.Api.Identity;
using ClarityClinical.Application.Admin;
using ClarityClinical.Infrastructure.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClarityClinical.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrator")]
[Route("api/admin")]
public sealed class AdminGovernanceController(
    IAdminReadService adminReadService,
    GovernanceSeeder governanceSeeder) : ControllerBase
{
    [HttpGet("governance/guidelines")]
    public async Task<IActionResult> GetGuidelines(CancellationToken cancellationToken)
    {
        await governanceSeeder.SeedAsync(cancellationToken);
        return Ok(await adminReadService.GetGuidelinesAsync(cancellationToken));
    }

    [HttpGet("governance/rules")]
    public async Task<IActionResult> GetRules(CancellationToken cancellationToken)
    {
        await governanceSeeder.SeedAsync(cancellationToken);
        return Ok(await adminReadService.GetRulesAsync(cancellationToken));
    }

    [HttpGet("audit")]
    public async Task<IActionResult> GetAudit(
        [FromQuery] Guid? consultationId,
        [FromQuery] string? action,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var visitorId = User.FindFirst(DemoClaimTypes.VisitorId)?.Value;
        if (string.IsNullOrWhiteSpace(visitorId)) return Unauthorized();
        return Ok(await adminReadService.GetAuditAsync(visitorId, consultationId, action, page, pageSize, cancellationToken));
    }
}
