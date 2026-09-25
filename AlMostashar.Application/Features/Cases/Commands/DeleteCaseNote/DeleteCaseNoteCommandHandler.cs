using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Cases.Commands.DeleteCaseNote;

public class DeleteCaseNoteCommandHandler : IRequestHandler<DeleteCaseNoteCommand, Result<string>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteCaseNoteCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<string>> Handle(DeleteCaseNoteCommand request, CancellationToken cancellationToken)
    {
        var caseEntity = await _db.Cases
            .FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);

        if (caseEntity is null)
            return Result<string>.Failure(new Error("Case.NotFound", Messages.Cases.NotFound));

        if (caseEntity.LawyerId != _currentUser.UserId)
            return Result<string>.Failure(new Error("Auth.Forbidden", Messages.Auth.NotCaseOwner));

        var note = await _db.CaseNotes
            .FirstOrDefaultAsync(n => n.Id == request.NoteId && n.CaseId == request.CaseId, cancellationToken);

        if (note is null)
            return Result<string>.Failure(new Error("CaseNote.NotFound", Messages.Cases.NoteNotFound));

        _db.CaseNotes.Remove(note);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(Messages.GeneralSuccess.NoteDeleted);
    }
}
