using AlMostashar.Api.SignalR;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Models;
using AlMostashar.Domain.ValueObject.Enum;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Api.Services
{
    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<AlMostasharHub> _hubContext;
        private readonly IConnectionTracker _connectionTracker;
        private readonly ILogger<SignalRNotificationService> _logger;

        public SignalRNotificationService(
            IHubContext<AlMostasharHub> hubContext,
            IConnectionTracker connectionTracker,
            ILogger<SignalRNotificationService> logger)
        {
            _hubContext = hubContext;
            _connectionTracker = connectionTracker;
            _logger = logger;
        }

        public async Task SendToAllAsync(
            string title,
            string message,
            NotificationType type,
            int? referenceId = null,
            string? senderName = null,
            CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.All.SendAsync(
                "ReceiveNotification",
                new
                {
                    title,
                    message,
                    type = type.ToString(),
                    referenceId,
                    senderName
                },
                cancellationToken);
        }

        public async Task SendToUserAsync(
            int userId,
            string title,
            string message,
            NotificationType type,
            int? referenceId = null,
            string? senderName = null,
            string? profileImage = null,
            CancellationToken cancellationToken = default)
        {
            var isActive = _connectionTracker.IsUserActive(userId);

            _logger.LogInformation(
                "[NOTIFY] SendToUserAsync - UserId={UserId}, IsUserActive={IsActive}, Type={Type}, Title={Title}",
                userId,
                isActive,
                type,
                title);

            if (isActive)
            {
                _logger.LogDebug(
                    "[NOTIFY] Sending via SignalR to UserId={UserId}",
                    userId);

                await _hubContext.Clients.User(userId.ToString())
                    .SendAsync(
                        "ReceiveNotification",
                        new
                        {
                            title,
                            message,
                            type = type.ToString(),
                            referenceId,
                            senderName,
                            profileImage
                        },
                        cancellationToken);
            }
            else
            {
                _logger.LogDebug(
                    "[NOTIFY] UserId={UserId} is not connected to SignalR. Notification will not be sent.",
                    userId);
            }
        }

        public async Task SendToGroupAsync(
            string groupName,
            string title,
            string message,
            NotificationType type,
            int? referenceId = null,
            string? senderName = null,
            CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.Group(groupName)
                .SendAsync(
                    "ReceiveNotification",
                    new
                    {
                        title,
                        message,
                        type = type.ToString(),
                        referenceId,
                        senderName
                    },
                    cancellationToken);
        }

        public async Task SendMessagesReadAsync(
            int chatId,
            int readByUserId,
            int notifyUserId,
            int lastReadMessageId,
            DateTime readAt,
            CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.User(notifyUserId.ToString())
                .SendAsync(
                    "MessagesRead",
                    new
                    {
                        chatId,
                        readByUserId,
                        lastReadMessageId,
                        readAt
                    },
                    cancellationToken);
        }

        public async Task NotifyPaymentStatusAsync(
            int userId,
            int invoiceId,
            bool isSuccess,
            CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync(
                    "ReceivePaymentStatus",
                    new
                    {
                        invoiceId,
                        isSuccess
                    },
                    cancellationToken);
        }

        public async Task TriggerIncomingCallAsync(
            int ReceiverId,
            IncomingCallDto incomingCallDto,
            CancellationToken cancellationToken)
        {
            var isUserActive = _connectionTracker.IsUserActive(ReceiverId);

            _logger.LogInformation(
                "[CALL] TriggerIncomingCallAsync - ReceiverId={ReceiverId}, IsActive={IsActive}, CallSessionId={CallSessionId}, CallType={CallType}",
                ReceiverId,
                isUserActive,
                incomingCallDto.CallSessionId,
                incomingCallDto.CallType);

            if (isUserActive)
            {
                _logger.LogInformation(
                    "[CALL] Sending via SignalR to ReceiverId={ReceiverId}",
                    ReceiverId);

                await _hubContext.Clients.User(ReceiverId.ToString())
                    .SendAsync(
                        "ReceiveIncomingCall",
                        incomingCallDto,
                        cancellationToken);
            }
            else
            {
                _logger.LogInformation(
                    "[CALL] ReceiverId={ReceiverId} is not connected to SignalR. Incoming call notification will not be sent.",
                    ReceiverId);
            }
        }

        public async Task SendCallAcceptedAsync(
            int userId,
            string message,
            CancellationToken cancellationToken)
        {
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync(
                    "CallAccepted",
                    new { message },
                    cancellationToken);
        }

        public async Task SendCallRejectedAsync(
            int userId,
            string message,
            CancellationToken cancellationToken)
        {
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync(
                    "CallRejected",
                    new { message },
                    cancellationToken);
        }

        public async Task SendCallEndedAsync(
            int userId,
            string message,
            CancellationToken cancellationToken)
        {
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync(
                    "CallEnded",
                    new { message },
                    cancellationToken);
        }
    }
}