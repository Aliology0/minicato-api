using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Events;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Application.Features.LegalService.EventHandlers
{
    /// <summary>
    /// Handles <see cref="LegalServiceCreatedEvent"/> by sending a real-time
    /// notification to all connected users about the new service.
    /// </summary>
    public class LegalServiceCreatedEventHandler : INotificationHandler<LegalServiceCreatedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<LegalServiceCreatedEventHandler> _logger;

        public LegalServiceCreatedEventHandler(
            INotificationService notificationService,
            ILogger<LegalServiceCreatedEventHandler> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Handle(LegalServiceCreatedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                await _notificationService.SendToAllAsync(
                    Messages.Notifications.NewServiceTitle,
                    Messages.Notifications.NewServiceBody(notification.Title),
                    NotificationType.General,
                    notification.LegalServiceId,null,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                // Log and swallow — notification failure should not roll back the save
                _logger.LogWarning(ex,
                    "Failed to send notification for LegalService {Id}",
                    notification.LegalServiceId);
            }
        }
    }
}
