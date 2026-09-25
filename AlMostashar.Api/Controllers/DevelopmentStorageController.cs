using AlMostashar.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers;

[ApiController]
[Route("api/development/storage")]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class DevelopmentStorageController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("{token}")]
    public IActionResult Download(
        string token,
        [FromServices] IWebHostEnvironment environment,
        [FromServices] IServiceProvider services)
    {
        if (!environment.IsDevelopment())
            return NotFound();
        var storage = services.GetRequiredService<DevelopmentLocalStorageService>();
        return storage.TryResolveDownload(token, out var path)
            ? PhysicalFile(path, "application/octet-stream")
            : NotFound();
    }
}
