using ClarityClinical.Api.Identity;
using ClarityClinical.Application.Consultations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClarityClinical.Api.Controllers;

[ApiController]
[Authorize(Roles = "Clinician")]
[Route("api/consultations")]
public sealed class ConsultationLifecycleController(
    ConsultationLifecycleService lifecycleService,
    DemoConsultationAccess demoAccess) : ControllerBase
{
    [HttpPost("{consultationId:guid}/start")]
    public async Task<IActionResult> Start(
        Guid consultationId,
        CancellationToken cancellationToken)
    {
        if (!await demoAccess.CanAccessAsync(consultationId, cancellationToken))
        {
            return ConsultationNotFound();
        }

        return await ExecuteAsync(() => lifecycleService.StartAsync(
            consultationId,
            DateTimeOffset.UtcNow,
            cancellationToken));
    }

    [HttpPost("{consultationId:guid}/complete")]
    public async Task<IActionResult> Complete(
        Guid consultationId,
        CancellationToken cancellationToken)
    {
        if (!await demoAccess.CanAccessAsync(consultationId, cancellationToken))
        {
            return ConsultationNotFound();
        }

        return await ExecuteAsync(() => lifecycleService.CompleteAsync(
            consultationId,
            DateTimeOffset.UtcNow,
            cancellationToken));
    }

    private async Task<IActionResult> ExecuteAsync(
        Func<Task<Domain.Consultations.Consultation>> operation)
    {
        try
        {
            var consultation = await operation();
            return Ok(new ConsultationStatusResponse(
                consultation.Id,
                consultation.Status.ToString()));
        }
        catch (KeyNotFoundException)
        {
            return ConsultationNotFound();
        }
        catch (InvalidOperationException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid consultation state",
                detail: exception.Message);
        }
    }

    private ObjectResult ConsultationNotFound() => Problem(
        statusCode: StatusCodes.Status404NotFound,
        title: "Consultation not found",
        detail: "The consultation was not found or is not available to this demo session.");

    public sealed record ConsultationStatusResponse(Guid Id, string Status);
}
