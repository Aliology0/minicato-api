using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Auth.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using static AlMostashar.Application.Helpers.UploadToStorage;
namespace AlMostashar.Application.Features.Auth.Commands.UploadIdentityDocuments
{
    public class UploadIdentityDocumentsCommandHandler
        : IRequestHandler<UploadIdentityDocumentsCommand, Result<UploadIdentityDocumentsResponseDto>>
    {
        private readonly IStorageService _storageService;

        public UploadIdentityDocumentsCommandHandler(IStorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task<Result<UploadIdentityDocumentsResponseDto>> Handle(
            UploadIdentityDocumentsCommand request,
            CancellationToken cancellationToken)
        {
            var response = new UploadIdentityDocumentsResponseDto();

            // 1. Define tasks without awaiting them immediately
            // Start all upload operations at the same time
            var ssnTask = request.SSN != null
                ? UploadAsync(request.SSN, _storageService)
                : Task.FromResult<string>(null!);

            var syndicateTask = request.SyndicateCard != null
                ? UploadAsync(request.SyndicateCard, _storageService)
                : Task.FromResult<string>(null!);

            var practiceTask = request.PracticeCertificates != null
                ? UploadAsync(request.PracticeCertificates, _storageService)
                : Task.FromResult<string>(null!);

            // 2. Wait for all tasks to complete in parallel
            await Task.WhenAll(ssnTask, syndicateTask, practiceTask);

            // 3. Assign results from completed tasks
            response.SSN_Url = await ssnTask;
            response.SyndicateCardUrl = await syndicateTask;
            response.PracticeCertificatesUrl = await practiceTask;

            return Result<UploadIdentityDocumentsResponseDto>.Success(response);
        }
    }
}
