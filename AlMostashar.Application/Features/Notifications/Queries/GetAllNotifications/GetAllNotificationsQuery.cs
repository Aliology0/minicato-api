using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.Notifications.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Notifications.Queries.GetAllNotifications;

public record GetAllNotificationsQuery(
    bool? IsRead,
    int? Cursor,
    int PageSize = 10
) : IRequest<Result<CursorPagedResult<NotificationDto>>>;
