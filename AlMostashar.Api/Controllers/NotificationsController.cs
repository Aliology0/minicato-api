using AlMostashar.Api.Helpers;
using AlMostashar.Application.Features.Notifications.Commands.RegisterFcmToken;
using AlMostashar.Application.Features.Notifications.Queries.GetAllNotifications;
using AlMostashar.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get all notifications for the currently authenticated user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAllNotifications(
        [FromQuery] bool? isRead,
        [FromQuery] int? cursor,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllNotificationsQuery(
            IsRead: isRead,
            Cursor: cursor,
            PageSize: pageSize);

        var result = await _mediator.Send(query, cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>
    /// Register or refresh the FCM device token for the currently authenticated user.
    /// The client should call this on every app launch to keep the token up to date.
    /// </summary>
    [HttpPost("fcm-token")]
    public async Task<IActionResult> RegisterFcmToken(
        [FromBody] RegisterFcmTokenCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>Mark all notifications (or up to a specific id) as read for the currently authenticated user.</summary>
    [HttpPut("mark-as-read")]
    public async Task<IActionResult> MarkAllAsRead(
        [FromQuery] int? lastReadId,
        CancellationToken cancellationToken = default)
    {
        var command = new MarkAllNotificationsAsReadCommand(lastReadId);
        var result = await _mediator.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }
}
