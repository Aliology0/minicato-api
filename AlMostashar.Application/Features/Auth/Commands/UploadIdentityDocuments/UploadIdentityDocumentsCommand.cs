using AlMostashar.Application.Features.Auth.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace AlMostashar.Application.Features.Auth.Commands.UploadIdentityDocuments
{
    public class UploadIdentityDocumentsCommand : IRequest<Result<UploadIdentityDocumentsResponseDto>>
    {
        public IFormFile? SSN { get; set; }
        public IFormFile? SyndicateCard { get; set; }
        public IFormFile? PracticeCertificates { get; set; }
    }
}
