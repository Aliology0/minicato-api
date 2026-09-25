using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;

public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, Result<int>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public MarkAllNotificationsAsReadCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        var updatedCount = await _db.MarkAllNotificationsAsReadAsync(_currentUser.UserId, request.LastReadId, cancellationToken);
        return Result<int>.Success(updatedCount);
    }
}
