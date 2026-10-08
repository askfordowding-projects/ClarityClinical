using ClarityClinical.Api.Identity;
using ClarityClinical.Application.Consultations.Workspace;
using ClarityClinical.Application.Transcripts;
using ClarityClinical.Domain.Transcripts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClarityClinical.Api.Controllers;

[ApiController]
[Authorize(Roles = "Clinician")]
[Route("api/consultations/{consultationId:guid}/transcript-segments")]
public sealed class TranscriptControlController(
    ManageTranscriptSegmentService transcriptService,
    GetConsultationWorkspaceQuery workspaceQuery,
    DemoConsultationAccess demoAccess) : ControllerBase
{
    [HttpPatch("{segmentId:guid}/correction")]
    public Task<IActionResult> Correct(
        Guid consultationId,
        Guid segmentId,
        CorrectTranscriptRequest request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(
            consultationId,
            () => transcriptService.CorrectAsync(
                consultationId,
                segmentId,
                request.CorrectedText,
                request.TranslatedText,
                cancellationToken),
            cancellationToken);

    [HttpPost("{segmentId:guid}/speaker")]
    public async Task<IActionResult> ChangeSpeaker(
        Guid consultationId,
        Guid segmentId,
        ChangeSpeakerRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ConsultationSpeakerRole>(request.SpeakerRole, true, out var speakerRole))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid speaker role",
                detail: "The requested transcript speaker role is not supported.");
        }

        return await ExecuteAsync(
            consultationId,
            () => transcriptService.ChangeSpeakerAsync(
                consultationId,
                segmentId,
                speakerRole,
                cancellationToken),
            cancellationToken);
    }

    [HttpPost("{segmentId:guid}/exclude")]
    public Task<IActionResult> ExcludeFromReasoning(
        Guid consultationId,
        Guid segmentId,
        TranscriptDispositionRequest request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(
            consultationId,
            () => transcriptService.ExcludeFromReasoningAsync(
                consultationId,
                segmentId,
                request.Reason,
                cancellationToken),
            cancellationToken);
    [HttpPost("{segmentId:guid}/redact")]
    public Task<IActionResult> Redact(
        Guid consultationId,
        Guid segmentId,
        TranscriptDispositionRequest request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(
            consultationId,
            () => transcriptService.RedactAsync(
                consultationId,
                segmentId,
                request.Reason,
                cancellationToken),
            cancellationToken);
    private async Task<IActionResult> ExecuteAsync(
        Guid consultationId,
        Func<Task> action,
        CancellationToken cancellationToken)
    {
        if (!await demoAccess.CanAccessAsync(consultationId, cancellationToken))
        {
            return NotFoundProblem();
        }

        try
        {
            await action();
            return Ok(await workspaceQuery.ExecuteAsync(consultationId, cancellationToken));
        }
        catch (KeyNotFoundException)
        {
            return NotFoundProblem();
        }
        catch (Exception exception) when (exception is InvalidOperationException or ArgumentException)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid transcript change",
                detail: exception.Message);
        }
    }

    private ObjectResult NotFoundProblem() => Problem(
        statusCode: StatusCodes.Status404NotFound,
        title: "Transcript segment not found",
        detail: "The transcript segment was not found or is not available to this demo session.");

    public sealed record CorrectTranscriptRequest(string CorrectedText, string? TranslatedText);
    public sealed record ChangeSpeakerRequest(string SpeakerRole);
    public sealed record TranscriptDispositionRequest(string Reason);
}