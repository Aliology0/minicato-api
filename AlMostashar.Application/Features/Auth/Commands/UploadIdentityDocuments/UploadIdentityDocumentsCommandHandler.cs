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
        private readonly IAppDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public UploadIdentityDocumentsCommandHandler(IStorageService storageService, IAppDbContext db, ICurrentUserService currentUser)
        {
            _storageService = storageService;
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<Result<UploadIdentityDocumentsResponseDto>> Handle(
            UploadIdentityDocumentsCommand request,
            CancellationToken cancellationToken)
        {
            var response = new UploadIdentityDocumentsResponseDto();

            // Upload files in parallel; if a supplied file upload fails, return an error.
            try
            {
                var frontTask = request.SSN != null
                    ? UploadAsync(request.SSN, _storageService)
                    : Task.FromResult<string?>(null);

                var backTask = request.SyndicateCard != null
                    ? UploadAsync(request.SyndicateCard, _storageService)
                    : Task.FromResult<string?>(null);

                var practiceTask = request.PracticeCertificates != null
                    ? UploadAsync(request.PracticeCertificates, _storageService)
                    : Task.FromResult<string?>(null);

                await Task.WhenAll(frontTask, backTask, practiceTask);

                var frontKey = await frontTask;
                var backKey = await backTask;
                var pracKey = await practiceTask;

                // Persist front/back ID to the authenticated user (Client) — do not assume Lawyer
                var user = await _db.Users.FindAsync(new object[] { _currentUser.UserId }, cancellationToken);
                if (user is null)
                    return Result<UploadIdentityDocumentsResponseDto>.Failure(new Error("Auth.UserNotFound", AlMostashar.Application.Common.Constants.Messages.Auth.UserNotFound));

                bool updated = false;
                if (!string.IsNullOrEmpty(frontKey))
                {
                    user.FrontIdUrl = frontKey; // store private key (not public URL)
                    updated = true;
                    response.SSN_Url = frontKey;
                }

                if (!string.IsNullOrEmpty(backKey))
                {
                    user.BackIdUrl = backKey;
                    updated = true;
                    response.SyndicateCardUrl = backKey;
                }

                if (!string.IsNullOrEmpty(pracKey))
                {
                    // PracticeCertificates for clients map to BackIdUrl or a new field; store in BackIdUrl if intended.
                    user.BackIdUrl = pracKey;
                    updated = true;
                    response.PracticeCertificatesUrl = pracKey;
                }

                if (updated)
                {
                    _db.Users.Update(user);
                    await _db.SaveChangesAsync(cancellationToken);
                }

                return Result<UploadIdentityDocumentsResponseDto>.Success(response);
            }
            catch (Exception)
            {
                var err = new Error("Auth.UploadFailed", "Failed to upload identity documents. Please try again.", null, "فشل تحميل مستندات الهوية. يرجى المحاولة مرة أخرى.");
                return Result<UploadIdentityDocumentsResponseDto>.Failure(err);
            }
        }
    }
}
