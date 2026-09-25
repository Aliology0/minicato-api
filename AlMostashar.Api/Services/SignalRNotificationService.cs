using AlMostashar.Api.SignalR;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Models;
using AlMostashar.Domain.ValueObject.Enum;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Api.Services
{
    /// <summary>
    /// SignalR-based implementation of <see cref="INotificationService"/>.
    /// Sends real-time events via SignalR when the user is online,
    /// and falls back to FCM push notifications via <see cref="IFcmService"/> when offline.
    /// </summary>
    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<AlMostasharHub> _hubContext;
        private readonly IConnectionTracker _connectionTracker;
        private readonly IFcmService _fcmService;
        private readonly ILogger<SignalRNotificationService> _logger;

        public SignalRNotificationService(
            IHubContext<AlMostasharHub> hubContext,
            IConnectionTracker connectionTracker,
            IFcmService fcmService,
            ILogger<SignalRNotificationService> logger)
        {
            _hubContext = hubContext;
            _connectionTracker = connectionTracker;
            _fcmService = fcmService;
            _logger = logger;
        }

        public async Task SendToAllAsync(string title, string message, NotificationType type,
            int? referenceId = null, string? senderName = null,
            CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification",
                new { title, message, type = type.ToString(), referenceId, senderName },
                cancellationToken);
        }

        public async Task SendToUserAsync(int userId, string title, string message, NotificationType type,
            int? referenceId = null, string? senderName = null, string? profileImage = null,
            CancellationToken cancellationToken = default)
        {
            var isActive = _connectionTracker.IsUserActive(userId);

            _logger.LogInformation(
                "[NOTIFY] SendToUserAsync — UserId={UserId}, IsUserActive={IsActive}, Type={Type}, Title={Title}",
                userId, isActive, type, title);

            if (isActive)
            {
                _logger.LogDebug("[NOTIFY] Sending via SignalR to UserId={UserId}", userId);
                await _hubContext.Clients.User(userId.ToString())
                    .SendAsync("ReceiveNotification",
                        new { title, message, type = type.ToString(), referenceId, senderName, profileImage },
                        cancellationToken);
            }
            else
            {
                _logger.LogDebug("[NOTIFY] Sending via FCM to UserId={UserId}", userId);
                var data = new Dictionary<string, string> {
                    {"title", title },
                    {"message", message },
                    {"type", type.ToString() },
                    {"referenceId", referenceId?.ToString() ?? "0" },
                    {"senderName", senderName ?? "" },
                    {"profileImage", profileImage ?? "" }
                };
                await _fcmService.SendToUserByIdAsync(userId, data, cancellationToken);
            }
        }

        public async Task SendToGroupAsync(string groupName, string title, string message, NotificationType type,
            int? referenceId = null, string? senderName = null,
            CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.Group(groupName)
                .SendAsync("ReceiveNotification",
                    new { title, message, type = type.ToString(), referenceId, senderName },
                    cancellationToken);
        }

        public async Task SendMessagesReadAsync(int chatId, int readByUserId, int notifyUserId,
            int lastReadMessageId, DateTime readAt,
            CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.User(notifyUserId.ToString())
                .SendAsync("MessagesRead",
                    new { chatId, readByUserId, lastReadMessageId, readAt },
                    cancellationToken);
        }

        public async Task NotifyPaymentStatusAsync(int userId, int invoiceId, bool isSuccess,
            CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("ReceivePaymentStatus",
                    new { invoiceId, isSuccess },
                    cancellationToken);
        }

        public async Task TriggerIncomingCallAsync(int ReceiverId, IncomingCallDto incomingCallDto, CancellationToken cancellationToken)
        {
            var isUserActive = _connectionTracker.IsUserActive(ReceiverId);

            _logger.LogInformation(
                "[CALL] TriggerIncomingCallAsync — ReceiverId={ReceiverId}, IsUserActive={IsActive}, CallSessionId={CallSessionId}, CallType={CallType}",
                ReceiverId, isUserActive, incomingCallDto.CallSessionId, incomingCallDto.CallType);

            // If the user has an active SignalR connection, send via SignalR for instant delivery
            if (isUserActive)
            {
                _logger.LogInformation("[CALL] Sending via SignalR to ReceiverId={ReceiverId}", ReceiverId);
                await _hubContext.Clients.User(ReceiverId.ToString())
                    .SendAsync("ReceiveIncomingCall", incomingCallDto, cancellationToken);
            }

            // ALWAYS send FCM for call notifications as a fallback.
            // SignalR may incorrectly report the user as active if the app was
            // just killed (WebSocket disconnect has not yet been detected).
            // The Flutter client must deduplicate by callSessionId.
            var data = new Dictionary<string, string> {
                {"callSessionId", incomingCallDto.CallSessionId },
                {"type", incomingCallDto.CallType },
                {"channelName", incomingCallDto.ChannelName },
                {"token", incomingCallDto.Token},
                {"callerId", incomingCallDto.CallerInfo.CallerId.ToString() },
                {"callerName", incomingCallDto.CallerInfo.CallerName},
                {"callerProfileImage", incomingCallDto.CallerInfo.ProfileImage },
            };

            _logger.LogInformation("[CALL] Sending via FCM to ReceiverId={ReceiverId} (always-on fallback)", ReceiverId);
            await _fcmService.SendCallNotificationAsync(ReceiverId, data, cancellationToken);
        }

        public async Task SendCallAcceptedAsync(int userId, string message, CancellationToken cancellationToken)
        {
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("CallAccepted", new { message }, cancellationToken);
        }

        public async Task SendCallRejectedAsync(int userId, string message, CancellationToken cancellationToken)
        {
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("CallRejected", new { message }, cancellationToken);
        }

        public async Task SendCallEndedAsync(int userId, string message, CancellationToken cancellationToken)
        {
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("CallEnded", new { message }, cancellationToken);
        }
    }
}
