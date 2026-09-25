using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.LegalService.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.LegalService.Commands.AddLegalService
{
    public class AddLegalServiceCommand : IRequest<Result<LegalServiceResponse>>, ITransactionalCommand
    {
        // Required
        public string Title { get; set; } = null!;
        public string Summary { get; set; } = null!;
        public string FullDescription { get; set; } = null!;
        public int ServiceType { get; set; }

        // Optional
        public string? IconUrl { get; set; }
        public string? RequiredDocuments { get; set; }
        public string? ExpectedDuration { get; set; }
    }
}
