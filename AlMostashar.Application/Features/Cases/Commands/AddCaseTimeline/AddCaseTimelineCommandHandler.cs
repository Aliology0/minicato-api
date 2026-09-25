using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Cases.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Cases.Commands.AddCaseTimeline;

public class AddCaseTimelineCommandHandler : IRequestHandler<AddCaseTimelineCommand, Result<CaseTimelineDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AddCaseTimelineCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<CaseTimelineDto>> Handle(AddCaseTimelineCommand request, CancellationToken cancellationToken)
    {
        var caseEntity = await _db.Cases
            .FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);

        if (caseEntity is null)
            return Result<CaseTimelineDto>.Failure(new Error("Case.NotFound", "Case not found."));

        if (caseEntity.LawyerId != _currentUser.UserId)
            return Result<CaseTimelineDto>.Failure(new Error("Auth.Forbidden", "You do not own this case."));

        var timeline = new CaseTimeline
        {
            CaseId = request.CaseId,
            Title = request.Title,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        _db.CaseTimelines.Add(timeline);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<CaseTimelineDto>.Success(new CaseTimelineDto
        {
            Id = timeline.Id,
            Title = timeline.Title,
            Content = timeline.Content,
            CreatedAt = timeline.CreatedAt
        });
    }
}
