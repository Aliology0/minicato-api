using AlMostashar.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AlMostashar.Api.Controllers;

[ApiController]
[Route("api/storage")]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class LocalStorageController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("download/{token}")]
    public IActionResult Download(string token, [FromServices] LocalFileStorageService storage)
    {
        if (!storage.TryResolveDownload(token, out var path))
            return NotFound();

        var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(path, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        return PhysicalFile(path, contentType);
    }
}
