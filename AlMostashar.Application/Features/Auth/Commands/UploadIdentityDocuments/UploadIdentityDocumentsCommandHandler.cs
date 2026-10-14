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
                ? UploadPublicAsync(request.SSN, _storageService)
                : Task.FromResult<string?>(null);

            var syndicateTask = request.SyndicateCard != null
                ? UploadPublicAsync(request.SyndicateCard, _storageService)
                : Task.FromResult<string?>(null);

            var practiceTask = request.PracticeCertificates != null
                ? UploadPublicAsync(request.PracticeCertificates, _storageService)
                : Task.FromResult<string?>(null);

            // 2. Wait for all tasks to complete in parallel
            await Task.WhenAll(ssnTask, syndicateTask, practiceTask);

            // 3. Assign results from completed tasks
            var ssnKey = await ssnTask;
            var syndKey = await syndicateTask;
            var pracKey = await practiceTask;

            response.SSN_Url = ssnKey is null ? null : _storageService.GetPublicUrl(ssnKey);
            response.SyndicateCardUrl = syndKey is null ? null : _storageService.GetPublicUrl(syndKey);
            response.PracticeCertificatesUrl = pracKey is null ? null : _storageService.GetPublicUrl(pracKey);

            return Result<UploadIdentityDocumentsResponseDto>.Success(response);
        }
    }
}
