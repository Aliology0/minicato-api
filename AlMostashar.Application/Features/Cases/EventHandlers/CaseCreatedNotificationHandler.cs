using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Events;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Application.Features.Cases.EventHandlers;

/// <summary>
/// Sends a notification to the lawyer when a case is created (client paid).
/// </summary>
public class CaseCreatedNotificationHandler : INotificationHandler<CaseCreatedEvent>
{
    private readonly IAppDbContext _db;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CaseCreatedNotificationHandler> _logger;

    public CaseCreatedNotificationHandler(
        IAppDbContext db,
        INotificationService notificationService,
        ILogger<CaseCreatedNotificationHandler> logger)
    {
        _db = db;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(CaseCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // جلب اسم العميل
            var client = await _db.Users
                .Where(u => u.Id == notification.ClientId)
                .Select(u => new { u.FullName, u.AvatarUrl })
                .FirstOrDefaultAsync(cancellationToken);

            var title = Messages.Notifications.CaseStartedTitle;
            var body = Messages.Notifications.CaseStartedBody(client?.FullName??"");

            // حفظ في الداتابيز
            var notificationEntity = new Notification
            {
                UserId = notification.LawyerId,
                Title = title,
                Description = body,
                Type = NotificationType.CaseUpdate,
                SourceId = notification.CaseId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            await _db.Notifications.AddAsync(notificationEntity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // إرسال real-time للمحامي
            await _notificationService.SendToUserAsync(
                notification.LawyerId,
                title,
                body,
                NotificationType.CaseUpdate,
                notification.CaseId,
                client?.FullName,
                client?.AvatarUrl,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to send CaseCreated notification to lawyer {LawyerId} for case {CaseId}",
                notification.LawyerId, notification.CaseId);
        }
    }
}
