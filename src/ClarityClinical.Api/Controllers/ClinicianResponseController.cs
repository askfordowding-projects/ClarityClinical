using ClarityClinical.Api.Identity;
using ClarityClinical.Application.Consultations.Responses;
using ClarityClinical.Domain.Assessments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClarityClinical.Api.Controllers;

[ApiController]
[Authorize(Roles = "Clinician")]
[Route("api/consultations/{consultationId:guid}/recommendations/{recommendationKey}/responses")]
public sealed class ClinicianResponseController(
    RecordClinicianResponseService responseService,
    DemoConsultationAccess demoAccess) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Record(
        Guid consultationId,
        string recommendationKey,
        ClinicianResponseRequest request,
        CancellationToken cancellationToken)
    {
        if (!await demoAccess.CanAccessAsync(consultationId, cancellationToken))
        {
            return NotFoundProblem();
        }

        if (!Enum.TryParse<ClinicianResponseType>(request.ResponseType, true, out var responseType)
            || responseType == ClinicianResponseType.NotReviewed)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid clinician response",
                detail: "Choose Accepted, Modified, Rejected or Deferred.");
        }

        try
        {
            var response = await responseService.RecordAsync(
                consultationId,
                recommendationKey,
                responseType,
                request.Rationale,
                request.ModifiedAction,
                cancellationToken);
            return Ok(new ClinicianResponseResponse(
                response.Id,
                response.RecommendationKey,
                response.ResponseType.ToString(),
                response.Rationale,
                response.ModifiedAction,
                response.RespondedAt));
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
                title: "Invalid clinician response",
                detail: exception.Message);
        }
    }

    private ObjectResult NotFoundProblem() => Problem(
        statusCode: StatusCodes.Status404NotFound,
        title: "Recommendation not found",
        detail: "The recommendation was not found or is not available to this demo session.");

    public sealed record ClinicianResponseRequest(
        string ResponseType,
        string? Rationale,
        string? ModifiedAction);

    public sealed record ClinicianResponseResponse(
        Guid Id,
        string RecommendationKey,
        string ResponseType,
        string? Rationale,
        string? ModifiedAction,
        DateTimeOffset RespondedAt);
}
