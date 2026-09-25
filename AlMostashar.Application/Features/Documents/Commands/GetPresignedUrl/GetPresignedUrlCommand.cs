using AlMostashar.Application.Features.Documents.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Documents.Commands.GetPresignedUrl
{
    public class GetPresignedUrlCommand : IRequest<Result<GetPresignedUrlResponseDto>>
    {
        public int DocumentId { get; set; }
        public int? ExpirationMinutes { get; set; }
    }
}
