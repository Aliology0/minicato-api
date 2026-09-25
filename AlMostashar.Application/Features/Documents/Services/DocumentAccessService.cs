using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Documents.Services;

public sealed class DocumentAccessService : IDocumentAccessService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DocumentAccessService(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<CaseDocuments>> GetForReadAsync(int documentId, CancellationToken cancellationToken)
    {
        var document = await _db.CaseDocuments
            .AsNoTracking()
             .Select(cd => new
             {
                 cd.Id,
                 cd.DocumentName,
                 cd.DocumentUrl,
                 cd.DocumentType,
                 cd.SizeInBytes,
                 cd.CreatedAt,
                 cd.ClientRequestId,
                 cd.CaseId,
                 cd.Case!.Chat!.ChatParticipants,
                 cd.UploadedByUserId,
                 cd.CleanupClaimToken,
                 cd.CleanupClaimedAt,
                 ReporterId = (int?)cd.Report!.ReporterId,
                 ClientId = (int?)cd.ClientRequest!.ClientId,
                 LawyerServiceLawyerId = (int?)cd.ClientRequest.LawyerServiceLawyerId
             })
                 .FirstOrDefaultAsync(value => value.Id == documentId, cancellationToken);




        if (document is null)
            return Result<CaseDocuments>.Failure(new Error("Document.NotFound", "Document was not found."));

        var userId = _currentUser.UserId;
        var isAdmin = await _db.Admins.AsNoTracking()
            .AnyAsync(value => value.Id == userId, cancellationToken);
        var isAuthorized = isAdmin
            || document.UploadedByUserId == userId
            || document.ReporterId == userId
            || document.ClientId == userId
            || document.LawyerServiceLawyerId == userId
            || document.ChatParticipants.Any(p => p.UserId == userId);

        if (!isAuthorized)
            if(document.LawyerServiceLawyerId == null)
            {
                bool isLawyer = await _db.Lawyers.AnyAsync(l => l.Id == userId);
                if (isLawyer)
                    isAuthorized = true;
            }
        
        return isAuthorized
            ? Result<CaseDocuments>.Success(new CaseDocuments 
            { 
                Id= document.Id,
                DocumentUrl = document.DocumentUrl,
                DocumentName= document.DocumentName,
                SizeInBytes = document.SizeInBytes,
                DocumentType= document.DocumentType,
                CreatedAt = document.CreatedAt,
                UploadedByUserId = document.UploadedByUserId,
                CleanupClaimedAt = document.CleanupClaimedAt,
                CleanupClaimToken = document.CleanupClaimToken,
                CaseId = document.CaseId,
                ClientRequestId = document.ClientRequestId,
                ReportId = document.ReporterId,
            })
            : Result<CaseDocuments>.Failure(new Error("Document.Forbidden", "You are not authorized to access this document."));
    }

    public async Task<Result<CaseDocuments>> GetUnlinkedForDeleteAsync(
        int documentId,
        CancellationToken cancellationToken)
    {
        var document = await _db.CaseDocuments
            .SingleOrDefaultAsync(value => value.Id == documentId, cancellationToken);

        if (document is null)
            return Result<CaseDocuments>.Failure(new Error("Document.NotFound", "Document was not found."));

        if (document.UploadedByUserId != _currentUser.UserId)
            return Result<CaseDocuments>.Failure(new Error("Document.Forbidden", "Only the upload owner can delete this document."));

        if (document.ClientRequestId.HasValue || document.CaseId.HasValue || document.ReportId.HasValue)
            return Result<CaseDocuments>.Failure(new Error(
                "Document.Linked",
                "Linked request, case, or report documents must be deleted through their dedicated workflow."));

        return Result<CaseDocuments>.Success(document);
    }
}
