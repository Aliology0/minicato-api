using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Events;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Application.Features.ClientRequests.EventHandlers;

public class RequestAcceptedEventHandler : INotificationHandler<RequestAcceptedEvent>
{
    private readonly IAppDbContext _db;
    private readonly INotificationService _notificationService;
    private readonly ILogger<RequestAcceptedEventHandler> _logger;

    public RequestAcceptedEventHandler(
        IAppDbContext db,
        INotificationService notificationService,
        ILogger<RequestAcceptedEventHandler> logger)
    {
        _db = db;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(RequestAcceptedEvent notification, CancellationToken cancellationToken)
    {
        // Idempotency: لو فيه فاتورة موجودة لنفس الطلب متنشئش تاني
        var existing = await _db.Invoices
            .FirstOrDefaultAsync(i => i.ClientRequestId == notification.ClientRequestId, cancellationToken);
        if (existing is not null) return;

        var invoice = new Invoice
        {
            ReferenceNumber = $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}",
            TotalAmount = notification.TotalAmount,
            PlatformFee = notification.PlatformFee,
            LawyerAmount = notification.LawyerAmount,
            Status = InvoiceStatus.Unpaid,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(7),
            ClientRequestId = notification.ClientRequestId,
        };

        await _db.Invoices.AddAsync(invoice, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        // إشعار العميل ان المحامي وافق وفي انتظار الدفع
        try
        {
            // جلب اسم المحامي
            var lawyer = notification.LawyerId.HasValue
                ? await _db.Users
                    .Where(u => u.Id == notification.LawyerId.Value)
                    .Select(u => new { u.FullName, ProfileImage = u.AvatarUrl })
                    .FirstOrDefaultAsync(cancellationToken) 
                : null;

            var title = Messages.Notifications.RequestAcceptedTitle;
            var body = Messages.Notifications.RequestAcceptedBody(lawyer?.FullName??"");

            // حفظ في الداتابيز
            var notificationEntity = new Notification
            {
                UserId = notification.ClientId,
                Title = title,
                Description = body,
                Type = NotificationType.RequestUpdate,
                SourceId = notification.ClientRequestId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            await _db.Notifications.AddAsync(notificationEntity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // إرسال real-time
            await _notificationService.SendToUserAsync(
                notification.ClientId,
                title,
                body,
                NotificationType.RequestUpdate,
                notification.ClientRequestId,
                lawyer?.FullName,
                lawyer?.ProfileImage,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to send RequestAccepted notification to client {ClientId} for request {RequestId}",
                notification.ClientId, notification.ClientRequestId);
        }
    }
}
