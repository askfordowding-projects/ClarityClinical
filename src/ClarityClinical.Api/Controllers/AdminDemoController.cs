using ClarityClinical.Api.Identity;
using ClarityClinical.Application.Admin;
using ClarityClinical.Application.Demo;
using ClarityClinical.Infrastructure.Demo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClarityClinical.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrator")]
[Route("api/admin/demo")]
public sealed class AdminDemoController(
    IDemoAdminReadService readService,
    IDemoSessionService sessionService,
    DemoScenarioSeeder scenarioSeeder) : ControllerBase
{
    [HttpGet("patients")]
    public async Task<IActionResult> GetPatients(CancellationToken cancellationToken)
    {
        await scenarioSeeder.SeedAsync(cancellationToken);
        return Ok(await readService.GetCanonicalPatientsAsync(cancellationToken));
    }

    [HttpGet("scenarios")]
    public async Task<IActionResult> GetScenarios(CancellationToken cancellationToken)
    {
        await scenarioSeeder.SeedAsync(cancellationToken);
        return Ok(await readService.GetCanonicalScenariosAsync(cancellationToken));
    }

    [HttpPost("sessions/{sessionId:guid}/reset")]
    public async Task<IActionResult> ResetSession(Guid sessionId, CancellationToken cancellationToken)
    {
        var visitorId = User.FindFirst(DemoClaimTypes.VisitorId)?.Value;
        if (string.IsNullOrWhiteSpace(visitorId)) return Unauthorized();
        try
        {
            var session = await sessionService.ResetAsync(sessionId, visitorId, cancellationToken);
            return Ok(new { session.Id, session.ConsultationId });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
