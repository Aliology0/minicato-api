using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Events;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Application.Features.Chat.EventHandlers;

/// <summary>
/// Sends a notification to the client when a chat is created,
/// letting them know they can now message the lawyer.
/// </summary>
public class ChatCreatedNotificationHandler : INotificationHandler<ChatCreatedEvent>
{
    private readonly IAppDbContext _db;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ChatCreatedNotificationHandler> _logger;

    public ChatCreatedNotificationHandler(
        IAppDbContext db,
        INotificationService notificationService,
        ILogger<ChatCreatedNotificationHandler> logger)
    {
        _db = db;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(ChatCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var lawyer = await _db.Users
                .Where(u => u.Id == notification.LawyerId)
                .Select(u => new { u.FullName, u.AvatarUrl })
                .FirstOrDefaultAsync(cancellationToken);

            var title = Messages.Notifications.ChatAvailableTitle;
            var body = Messages.Notifications.ChatAvailableBody(lawyer?.FullName??"");

            var notificationEntity = new Notification
            {
                UserId = notification.ClientId,
                Title = title,
                Description = body,
                Type = NotificationType.ChatAvailable,
                SourceId = notification.ChatId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            await _db.Notifications.AddAsync(notificationEntity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            await _notificationService.SendToUserAsync(
                notification.ClientId,
                title,
                body,
                NotificationType.ChatAvailable,
                notification.ChatId,
                lawyer?.FullName,
                lawyer?.AvatarUrl,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to send ChatCreated notification to client {ClientId} for chat {ChatId}",
                notification.ClientId, notification.ChatId);
        }
    }
}
