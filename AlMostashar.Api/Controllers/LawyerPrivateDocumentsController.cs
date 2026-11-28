using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Lawyers.Queries.GetPrivateDocument;
using AlMostashar.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace AlMostashar.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Lawyer")]
[Route("api/lawyers/{lawyerId:int}/documents")]
public sealed class LawyerPrivateDocumentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly LocalFileStorageService _storage;

    public LawyerPrivateDocumentsController(
        IMediator mediator,
        ICurrentUserService currentUser,
        LocalFileStorageService storage)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _storage = storage;
    }

    [HttpGet("{documentType}")]
    public async Task<IActionResult> Download(
        int lawyerId,
        LawyerPrivateDocumentType documentType,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(documentType))
            return BadRequest(new { message = "Invalid document type." });

        if (!User.IsInRole("Admin") && _currentUser.UserId != lawyerId)
            return Forbid();

        var result = await _mediator.Send(
            new GetLawyerPrivateDocumentQuery(lawyerId, documentType),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error?.Code switch
            {
                "Lawyer.NotFound" or "Lawyer.DocumentNotFound" => NotFound(),
                "Lawyer.DocumentTypeInvalid" => BadRequest(new { message = "Invalid document type." }),
                _ => BadRequest()
            };
        }

        string path;
        try
        {
            path = _storage.ResolvePrivateFilePath(result.Value!);
        }
        catch (ArgumentException)
        {
            return BadRequest(new { message = "Invalid stored document key." });
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }

        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(path, out var contentType))
            contentType = "application/octet-stream";

        Response.Headers.XContentTypeOptions = "nosniff";
        var downloadName = $"{documentType}{Path.GetExtension(path)}";
        return PhysicalFile(path, contentType, downloadName);
    }
}
