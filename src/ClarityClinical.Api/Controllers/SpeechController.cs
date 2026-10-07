using ClarityClinical.Application.Speech;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClarityClinical.Api.Controllers;

[ApiController]
[Authorize(Roles = "Clinician")]
[Route("api/speech")]
public sealed class SpeechController(
    ISpeechAuthorizationProvider authorizationProvider) : ControllerBase
{
    [HttpPost("authorization")]
    public async Task<IActionResult> GetAuthorization(CancellationToken cancellationToken)
    {
        try
        {
            var authorization = await authorizationProvider.GetAuthorizationAsync(cancellationToken);
            return Ok(authorization);
        }
        catch (SpeechServiceUnavailableException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Speech translation unavailable",
                detail: exception.Message);
        }
    }
}
