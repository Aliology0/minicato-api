using System.Security.Claims;
using AlMostashar.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/storage")]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class LocalStorageController : ControllerBase
{
    [HttpGet("download/{token}")]
    public IActionResult Download(string token, [FromServices] LocalFileStorageService storage)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (!storage.TryResolveDownload(token, userId, out var path))
            return NotFound();

        var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(path, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        Response.Headers.XContentTypeOptions = "nosniff";
        return PhysicalFile(path, contentType);
    }
}
