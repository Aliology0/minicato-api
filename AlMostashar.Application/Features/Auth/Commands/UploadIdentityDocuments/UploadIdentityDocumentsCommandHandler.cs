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
            // Track successful uploads so we can cleanup on DB failure.
            var uploadedKeys = new List<string>();
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

                if (!string.IsNullOrEmpty(frontKey)) uploadedKeys.Add(frontKey);
                if (!string.IsNullOrEmpty(backKey)) uploadedKeys.Add(backKey);
                if (!string.IsNullOrEmpty(pracKey)) uploadedKeys.Add(pracKey);

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

                // PracticeCertificates should not overwrite BackIdUrl. Only store practice certificates if the
                // authenticated user is a Lawyer and the application explicitly intends to persist practice docs
                // on the Lawyer entity. Respect role/ownership rules: do not assume the current user is a lawyer.
                if (!string.IsNullOrEmpty(pracKey))
                {
                    // If the user is a Lawyer, persist to PracticeCertificatesUrl on the Lawyer entity instead of User
                    if (user is Domain.Entities.Lawyer)
                    {
                        var lawyer = await _db.Lawyers.FindAsync(new object[] { _currentUser.UserId }, cancellationToken);
                        if (lawyer is not null)
                        {
                            // Persist the private key; the public URL will be generated when needed by GetPublicUrl or presigned URLs
                            lawyer.PracticeCertificatesUrl = pracKey;
                            _db.Lawyers.Update(lawyer);
                            response.PracticeCertificatesUrl = pracKey;
                            updated = true;
                        }
                    }
                    else
                    {
                        // Do not persist practice certificates for Clients. Instead return it in response only.
                        response.PracticeCertificatesUrl = pracKey;
                    }
                }

                if (updated)
                {
                    _db.Users.Update(user);
                    await _db.SaveChangesAsync(cancellationToken);
                }

                return Result<UploadIdentityDocumentsResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                // If DB persistence failed after successful uploads, attempt to remove the uploaded objects.
                if (uploadedKeys.Count > 0)
                {
                    foreach (var k in uploadedKeys)
                    {
                        try
                        {
                            await _storageService.DeleteFileAsync(k);
                        }
                        catch
                        {
                            // Do not throw; cleanup best-effort only.
                        }
                    }
                }

                var err = new Error("Upload.IdentityDocumentsFailed", "Failed to upload identity documents. Please try again.", null, "فشل تحميل مستندات الهوية. يرجى المحاولة مرة أخرى.");
                return Result<UploadIdentityDocumentsResponseDto>.Failure(err);
            }
        }
    }
}
