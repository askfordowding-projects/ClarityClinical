using ClarityClinical.Api.Identity;
using ClarityClinical.Application.Consultations.Workspace;
using ClarityClinical.Domain.Consultations;
using ClarityClinical.Domain.Transcripts;
using ClarityClinical.Application.Transcripts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClarityClinical.Api.Controllers;

[ApiController]
[Authorize(Roles = "Clinician")]
[Route("api/consultations/{consultationId:guid}")]
public sealed class ConsultationWorkspaceController(
    GetConsultationWorkspaceQuery workspaceQuery,
    AddClinicalFactService addClinicalFactService,
    ExcludeClinicalFactService excludeClinicalFactService,
    AddTranscriptSegmentService addTranscriptSegmentService,
    DemoConsultationAccess demoAccess) : ControllerBase
{
    [HttpGet("workspace")]
    public async Task<IActionResult> GetWorkspace(
        Guid consultationId,
        CancellationToken cancellationToken)
    {
        if (!await demoAccess.CanAccessAsync(consultationId, cancellationToken))
        {
            return NotFoundProblem();
        }

        try
        {
            return Ok(await workspaceQuery.ExecuteAsync(consultationId, cancellationToken));
        }
        catch (KeyNotFoundException)
        {
            return NotFoundProblem();
        }
    }

    [HttpPost("patient-reports")]
    public Task<IActionResult> AddPatientReport(
        Guid consultationId,
        ClinicalFactRequest request,
        CancellationToken cancellationToken) =>
        AddFactAndReturnWorkspaceAsync(
            consultationId,
            request,
            ClinicalFactSource.PatientReport,
            cancellationToken);

    [HttpPost("observations")]
    public Task<IActionResult> AddObservation(
        Guid consultationId,
        ClinicalFactRequest request,
        CancellationToken cancellationToken) =>
        AddFactAndReturnWorkspaceAsync(
            consultationId,
            request,
            ClinicalFactSource.ClinicalObservation,
            cancellationToken);

    [HttpPost("transcript-segments")]
    public async Task<IActionResult> AddTranscriptSegment(
        Guid consultationId,
        TranscriptSegmentRequest request,
        CancellationToken cancellationToken)
    {
        if (!await demoAccess.CanAccessAsync(consultationId, cancellationToken))
        {
            return NotFoundProblem();
        }

        if (!Enum.TryParse<ConsultationSpeakerRole>(request.SpeakerRole, true, out var speakerRole))
        {
            return Problem(statusCode: 400, title: "Invalid speaker role");
        }

        try
        {
            await addTranscriptSegmentService.AddAsync(
                consultationId,
                speakerRole,
                request.SourceLanguage,
                request.OriginalText,
                request.TranslatedText,
                request.RecognitionConfidence,
                cancellationToken);
            return Ok(await workspaceQuery.ExecuteAsync(consultationId, cancellationToken));
        }
        catch (KeyNotFoundException)
        {
            return NotFoundProblem();
        }
        catch (InvalidOperationException exception)
        {
            return Problem(statusCode: 400, title: "Invalid consultation state", detail: exception.Message);
        }
        catch (ArgumentException exception)
        {
            return Problem(statusCode: 400, title: "Invalid transcript segment", detail: exception.Message);
        }
    }
    [HttpPost("clinical-facts/{factId:guid}/exclude")]
    public async Task<IActionResult> ExcludeClinicalFact(
        Guid consultationId,
        Guid factId,
        CancellationToken cancellationToken)
    {
        if (!await demoAccess.CanAccessAsync(consultationId, cancellationToken))
        {
            return NotFoundProblem();
        }

        try
        {
            await excludeClinicalFactService.ExcludeAsync(
                consultationId,
                factId,
                cancellationToken);
            return Ok(await workspaceQuery.ExecuteAsync(consultationId, cancellationToken));
        }
        catch (KeyNotFoundException)
        {
            return NotFoundProblem();
        }
        catch (InvalidOperationException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid consultation state",
                detail: exception.Message);
        }
    }
    private async Task<IActionResult> AddFactAndReturnWorkspaceAsync(
        Guid consultationId,
        ClinicalFactRequest request,
        ClinicalFactSource source,
        CancellationToken cancellationToken)
    {
        if (!await demoAccess.CanAccessAsync(consultationId, cancellationToken))
        {
            return NotFoundProblem();
        }

        try
        {
            await addClinicalFactService.AddAsync(
                consultationId,
                request.Code,
                request.DisplayText,
                source,
                cancellationToken);
            return Ok(await workspaceQuery.ExecuteAsync(consultationId, cancellationToken));
        }
        catch (KeyNotFoundException)
        {
            return NotFoundProblem();
        }
        catch (InvalidOperationException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid consultation state",
                detail: exception.Message);
        }
        catch (ArgumentException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid clinical fact",
                detail: exception.Message);
        }
    }

    private ObjectResult NotFoundProblem() => Problem(
        statusCode: StatusCodes.Status404NotFound,
        title: "Clinical workspace not found",
        detail: "The consultation was not found or is not available to this demo session.");

    public sealed record ClinicalFactRequest(string Code, string DisplayText);

    public sealed record TranscriptSegmentRequest(
        string SpeakerRole,
        string SourceLanguage,
        string OriginalText,
        string? TranslatedText,
        double? RecognitionConfidence);
}
