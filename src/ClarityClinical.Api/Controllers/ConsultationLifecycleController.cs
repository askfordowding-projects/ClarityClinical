using ClarityClinical.Application.Consultations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClarityClinical.Api.Controllers;

[ApiController]
[Authorize(Roles = "Clinician")]
[Route("api/consultations")]
public sealed class ConsultationLifecycleController(
    ConsultationLifecycleService lifecycleService) : ControllerBase
{
    [HttpPost("{consultationId:guid}/start")]
    public Task<IActionResult> Start(
        Guid consultationId,
        CancellationToken cancellationToken) =>
        ExecuteAsync(
            () => lifecycleService.StartAsync(
                consultationId,
                DateTimeOffset.UtcNow,
                cancellationToken));

    [HttpPost("{consultationId:guid}/complete")]
    public Task<IActionResult> Complete(
        Guid consultationId,
        CancellationToken cancellationToken) =>
        ExecuteAsync(
            () => lifecycleService.CompleteAsync(
                consultationId,
                DateTimeOffset.UtcNow,
                cancellationToken));

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
        catch (KeyNotFoundException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Consultation not found",
                detail: exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid consultation state",
                detail: exception.Message);
        }
    }

    public sealed record ConsultationStatusResponse(Guid Id, string Status);
}
