using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Cases.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Cases.Commands.UploadCaseDocument;

public class UploadCaseDocumentCommandHandler : IRequestHandler<UploadCaseDocumentCommand, Result<CaseDocumentDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UploadCaseDocumentCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<CaseDocumentDto>> Handle(UploadCaseDocumentCommand request, CancellationToken cancellationToken)
    {
        var requesterId = _currentUser.UserId;

        var caseEntity = await _db.Cases
            .Include(c => c.CaseClientRequest)
                .ThenInclude(ccr => ccr!.ClientRequest)
            .FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);

        if (caseEntity is null)
            return Result<CaseDocumentDto>.Failure(new Error("Case.NotFound", Messages.Cases.NotFound));

        // Allow if requester is the lawyer OR the linked client
        var isLawyer = caseEntity.LawyerId == requesterId;
        var isClient = caseEntity.CaseClientRequest?.ClientRequest?.ClientId == requesterId;

        if (!isLawyer && !isClient)
            return Result<CaseDocumentDto>.Failure(new Error("Auth.Forbidden", Messages.Auth.CannotUploadToCase));

        var document = await _db.CaseDocuments.FirstOrDefaultAsync(d =>
            d.Id == request.DocumentId && d.UploadedByUserId == requesterId &&
            d.CaseId == null && d.ClientRequestId == null && d.ReportId == null && d.CleanupClaimToken == null, cancellationToken);
        if (document is null)
            return Result<CaseDocumentDto>.Failure(new Error("CaseDocument.Invalid", "Document is missing, owned by another user, or already linked."));
        document.CaseId = request.CaseId;
        await _db.SaveChangesAsync(cancellationToken);

        return Result<CaseDocumentDto>.Success(new CaseDocumentDto
        {
            Id = document.Id,
            DocumentName = document.DocumentName,
            CreatedAt = document.CreatedAt
        });
    }
}
