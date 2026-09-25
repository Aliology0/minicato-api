using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Cases.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Cases.Commands.AddCaseNote;

public class AddCaseNoteCommandHandler : IRequestHandler<AddCaseNoteCommand, Result<CaseNoteDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AddCaseNoteCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<CaseNoteDto>> Handle(AddCaseNoteCommand request, CancellationToken cancellationToken)
    {
        var caseEntity = await _db.Cases
            .FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);

        if (caseEntity is null)
            return Result<CaseNoteDto>.Failure(new Error("Case.NotFound", "Case not found."));

        if (caseEntity.LawyerId != _currentUser.UserId)
            return Result<CaseNoteDto>.Failure(new Error("Auth.Forbidden", "You do not own this case."));

        var note = new CaseNotes
        {
            CaseId = request.CaseId,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        _db.CaseNotes.Add(note);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<CaseNoteDto>.Success(new CaseNoteDto
        {
            Id = note.Id,
            Content = note.Content,
            CreatedAt = note.CreatedAt
        });
    }
}
