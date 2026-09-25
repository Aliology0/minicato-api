using AlMostashar.Api.Helpers;
using AlMostashar.Application.Features.Documents.Commands.DeleteDocument;
using AlMostashar.Application.Features.Documents.Commands.GetPresignedUrl;
using AlMostashar.Application.Features.Documents.Commands.GetPresignedUrlByPath;
using AlMostashar.Application.Features.Documents.Commands.UploadDocument;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers
{
    [Route("api/documents")]
    [ApiController]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DocumentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Upload a single document to S3 storage.</summary>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDocument(
            [FromForm] UploadDocumentCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Get a temporary presigned URL for downloading a stored file.</summary>
        [HttpPost("presigned-url")]
        public async Task<IActionResult> GetPresignedUrl(
            [FromBody] GetPresignedUrlCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Delete a document from S3 storage.</summary>
        [HttpDelete("{documentId:int}")]
        public async Task<IActionResult> DeleteDocument(
            int documentId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new DeleteDocumentCommand { DocumentId = documentId },
                cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>Get a temporary presigned URL for downloading a stored file.</summary>
        [Authorize(Roles ="Admin")]
        [HttpPost("PreSignedUrl")]
        public async Task<IActionResult> GetPresignedUrlByPath(
            [FromBody] GetPresignedUrlByPathCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

    }
}
