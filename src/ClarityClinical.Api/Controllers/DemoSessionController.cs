using ClarityClinical.Api.Identity;
using ClarityClinical.Application.Demo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClarityClinical.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/demo/sessions")]
public sealed class DemoSessionController(
    IDemoSessionService demoSessionService,
    DemoConsultationAccess demoAccess) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateDemoSessionRequest request,
        CancellationToken cancellationToken)
    {
        var visitorId = demoAccess.VisitorId;
        if (string.IsNullOrWhiteSpace(visitorId))
        {
            return Unauthorized();
        }

        try
        {
            var session = await demoSessionService.CreateAsync(
                request.ScenarioKey,
                visitorId,
                cancellationToken);
            return Ok(ToResponse(session));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFoundProblem(exception.Message);
        }
    }

    [HttpPost("{sessionId:guid}/reset")]
    public async Task<IActionResult> Reset(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var visitorId = demoAccess.VisitorId;
        if (string.IsNullOrWhiteSpace(visitorId))
        {
            return Unauthorized();
        }

        try
        {
            var session = await demoSessionService.ResetAsync(
                sessionId,
                visitorId,
                cancellationToken);
            return Ok(ToResponse(session));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFoundProblem(exception.Message);
        }
    }

    private ObjectResult NotFoundProblem(string detail) => Problem(
        statusCode: StatusCodes.Status404NotFound,
        title: "Demo session not found",
        detail: detail);

    private static DemoSessionResponse ToResponse(Domain.Demo.DemoSession session) =>
        new(session.Id, session.ConsultationId, session.ExpiresAt);

    public sealed record CreateDemoSessionRequest(string ScenarioKey);
    public sealed record DemoSessionResponse(Guid Id, Guid ConsultationId, DateTimeOffset ExpiresAt);
}
