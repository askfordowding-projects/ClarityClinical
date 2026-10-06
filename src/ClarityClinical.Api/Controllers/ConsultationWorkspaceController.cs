using ClarityClinical.Application.Consultations.Workspace;
using ClarityClinical.Domain.Consultations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClarityClinical.Api.Controllers;

[ApiController]
[Authorize(Roles = "Clinician")]
[Route("api/consultations/{consultationId:guid}")]
public sealed class ConsultationWorkspaceController(
    GetConsultationWorkspaceQuery workspaceQuery,
    AddClinicalFactService addClinicalFactService) : ControllerBase
{
    [HttpGet("workspace")]
    public async Task<IActionResult> GetWorkspace(
        Guid consultationId,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await workspaceQuery.ExecuteAsync(consultationId, cancellationToken));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFoundProblem(exception.Message);
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

    private async Task<IActionResult> AddFactAndReturnWorkspaceAsync(
        Guid consultationId,
        ClinicalFactRequest request,
        ClinicalFactSource source,
        CancellationToken cancellationToken)
    {
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
        catch (KeyNotFoundException exception)
        {
            return NotFoundProblem(exception.Message);
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

    private ObjectResult NotFoundProblem(string detail) =>
        Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Clinical workspace not found",
            detail: detail);

    public sealed record ClinicalFactRequest(string Code, string DisplayText);
}
