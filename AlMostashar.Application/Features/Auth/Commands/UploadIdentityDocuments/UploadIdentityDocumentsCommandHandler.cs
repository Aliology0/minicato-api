using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Auth.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using static AlMostashar.Application.Helpers.UploadToStorage;

namespace AlMostashar.Application.Features.Auth.Commands.UploadIdentityDocuments;

public sealed class UploadIdentityDocumentsCommandHandler
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
        var uploadedKeys = new List<string>();
        try
        {
            string? ssnKey = null;
            if (request.SSN is not null)
            {
                ssnKey = await UploadAsync(request.SSN, _storageService);
                uploadedKeys.Add(ssnKey);
            }
            string? syndicateKey = null;
            if (request.SyndicateCard is not null)
            {
                syndicateKey = await UploadAsync(request.SyndicateCard, _storageService);
                uploadedKeys.Add(syndicateKey);
            }

            var user = await _db.Users.FindAsync(new object[] { _currentUser.UserId }, cancellationToken);
            if (user is null)
            {
                await CleanupUploadedKeysAsync(uploadedKeys);
                return Result<UploadIdentityDocumentsResponseDto>.Failure(
                    new Error("Auth.UserNotFound", AlMostashar.Application.Common.Constants.Messages.Auth.UserNotFound));
            }

            if (ssnKey is not null)
            {
                user.FrontIdUrl = ssnKey;
                response.SSN_Url = ssnKey;
            }
            if (syndicateKey is not null)
            {
                user.BackIdUrl = syndicateKey;
                response.SyndicateCardUrl = syndicateKey;
            }

            if (ssnKey is not null || syndicateKey is not null)
            {
                _db.Users.Update(user);
                await _db.SaveChangesAsync(cancellationToken);
            }

            return Result<UploadIdentityDocumentsResponseDto>.Success(response);
        }
        catch (OperationCanceledException)
        {
            await CleanupUploadedKeysAsync(uploadedKeys);
            throw;
        }
        catch (Exception exception)
        {
            await CleanupUploadedKeysAsync(uploadedKeys);
            System.Diagnostics.Trace.TraceError("Identity document upload failed: {0}", exception);
            var err = new Error("Upload.IdentityDocumentsFailed", "Failed to upload identity documents. Please try again.", null, "فشل تحميل مستندات الهوية. يرجى المحاولة مرة أخرى.");
            return Result<UploadIdentityDocumentsResponseDto>.Failure(err);
        }
    }

    private async Task CleanupUploadedKeysAsync(IEnumerable<string> keys)
    {
        foreach (var key in keys.Distinct(StringComparer.Ordinal))
        {
            try
            {
                var cleanup = await _storageService.DeleteFileAsync(key);
                if (!cleanup.Success)
                    System.Diagnostics.Trace.TraceError("Identity document cleanup failed: {0}", cleanup.Message);
            }
            catch (Exception cleanupException)
            {
                System.Diagnostics.Trace.TraceError("Identity document cleanup failed: {0}", cleanupException);
            }
        }
    }
}
