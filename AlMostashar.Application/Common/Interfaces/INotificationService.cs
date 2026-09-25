using AlMostashar.Application.Common.Models;
using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Application.Common.Interfaces
{
    /// <summary>
    /// Abstraction for sending real-time notifications to connected clients.
    /// Responsible ONLY for delivery. Persistence is handled by the caller.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>Send a notification to all connected clients (broadcast only, not persisted).</summary>
        Task SendToAllAsync(string title, string message, NotificationType type,
            int? referenceId = null, string? senderName = null,
            CancellationToken cancellationToken = default);

        /// <summary>Send a notification to a specific user by their ID.</summary>
        Task SendToUserAsync(int userId, string title, string message, NotificationType type,
            int? referenceId = null, string? senderName = null, string? ProfileImage = null,
            CancellationToken cancellationToken = default);

        /// <summary>Send a notification to a group of users.</summary>
        Task SendToGroupAsync(string groupName, string title, string message, NotificationType type,
            int? referenceId = null, string? senderName = null,
            CancellationToken cancellationToken = default);

        /// <summary>Notify the message sender that their messages have been read by the receiver.</summary>
        Task SendMessagesReadAsync(int chatId, int readByUserId, int notifyUserId,
            int lastReadMessageId, DateTime readAt,
            CancellationToken cancellationToken = default);

        /// <summary>Notify a user about the status of their payment.</summary>
        Task NotifyPaymentStatusAsync(int userId, int invoiceId, bool isSuccess,
            CancellationToken cancellationToken = default);

        Task TriggerIncomingCallAsync(int ReceiverId, IncomingCallDto incomingCallDto, CancellationToken cancellationToken);

        /// <summary>Notify the caller that the call has been accepted.</summary>
        Task SendCallAcceptedAsync(int userId, string message, CancellationToken cancellationToken);

        /// <summary>Notify the caller that the call has been rejected.</summary>
        Task SendCallRejectedAsync(int userId, string message, CancellationToken cancellationToken);

        /// <summary>Notify the other participant that the call has ended.</summary>
        Task SendCallEndedAsync(int userId, string message, CancellationToken cancellationToken);
    }
}
