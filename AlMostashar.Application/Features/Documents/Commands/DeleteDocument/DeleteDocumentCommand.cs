using AlMostashar.Application.Features.Documents.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Documents.Commands.DeleteDocument
{
    public class DeleteDocumentCommand : IRequest<Result<DeleteDocumentResponseDto>>
    {
        public int DocumentId { get; set; }
    }
}
