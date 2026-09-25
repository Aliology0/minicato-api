using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Cases.Commands.DeleteCaseDocument;

public class DeleteCaseDocumentCommandHandler : IRequestHandler<DeleteCaseDocumentCommand, Result<string>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteCaseDocumentCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<string>> Handle(DeleteCaseDocumentCommand request, CancellationToken cancellationToken)
    {
        var caseEntity = await _db.Cases
            .FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);

        if (caseEntity is null)
            return Result<string>.Failure(new Error("Case.NotFound", Messages.Cases.NotFound));

        if (caseEntity.LawyerId != _currentUser.UserId)
            return Result<string>.Failure(new Error("Auth.Forbidden", Messages.Auth.NotCaseOwner));

        var document = await _db.CaseDocuments
            .FirstOrDefaultAsync(d => d.Id == request.DocumentId && d.CaseId == request.CaseId, cancellationToken);

        if (document is null)
            return Result<string>.Failure(new Error("CaseDocument.NotFound", Messages.Cases.DocumentNotFound));

        _db.CaseDocuments.Remove(document);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(Messages.GeneralSuccess.DocumentDeleted);
    }
}
