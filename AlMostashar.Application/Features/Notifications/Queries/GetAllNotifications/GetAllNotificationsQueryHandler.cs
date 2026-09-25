using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.Notifications.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Notifications.Queries.GetAllNotifications;

public class GetAllNotificationsQueryHandler : IRequestHandler<GetAllNotificationsQuery, Result<CursorPagedResult<NotificationDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetAllNotificationsQueryHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<CursorPagedResult<NotificationDto>>> Handle(GetAllNotificationsQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 50);

        var query = _db.Notifications
            .Where(n => n.UserId == _currentUser.UserId);

        if (request.IsRead.HasValue)
        {
            query = query.Where(n => n.IsRead == request.IsRead.Value);
        }

        // Ordered by descending Id (nearly sent / most recent first)
        query = query.OrderByDescending(n => n.Id);

        if (request.Cursor.HasValue)
        {
            query = query.Where(n => n.Id < request.Cursor.Value);
        }

        var items = await query
            .Take(pageSize + 1) // fetch one extra to detect HasMore
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                SourceId = n.SourceId,
                Type = n.Type.ToString(),
                Title = n.Title,
                Description = n.Description,
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead
            })
            .ToListAsync(cancellationToken);

        var hasMore = items.Count > pageSize;
        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
        }

        var result = new CursorPagedResult<NotificationDto>
        {
            Items = items,
            HasMore = hasMore,
            NextCursor = hasMore && items.Count > 0 ? items[^1].Id : null
        };

        return Result<CursorPagedResult<NotificationDto>>.Success(result);
    }
}
