using ClarityClinical.Application.SystemInfo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClarityClinical.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/system")]
public sealed class SystemInfoController(
    IBuildMetadataService metadataService,
    IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet("build")]
    public async Task<IActionResult> GetBuild(CancellationToken cancellationToken) =>
        Ok(await metadataService.GetAsync(environment.EnvironmentName, cancellationToken));
}
