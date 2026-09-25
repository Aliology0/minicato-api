using AlMostashar.Application.Features.Documents.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace AlMostashar.Application.Features.Documents.Commands.UploadDocument
{
    public class UploadDocumentCommand : IRequest<Result<UploadDocumentResponseDto>>
    {
        public IFormFile File { get; set; } = null!;
    }
}
