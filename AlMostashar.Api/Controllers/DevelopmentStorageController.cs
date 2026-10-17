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
        if (!storage.TryResolveDownload(token, out var path))
            return NotFound();

        // Safely map file extension to content type
        var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(path, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        return PhysicalFile(path, contentType);
    }
}
