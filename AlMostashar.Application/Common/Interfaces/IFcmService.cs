namespace AlMostashar.Application.Common.Interfaces
{
    /// <summary>
    /// Abstraction for sending Firebase Cloud Messaging (FCM) push notifications to mobile devices.
    /// Responsible ONLY for delivery. Token storage and persistence are handled by the caller.
    /// </summary>
    public interface IFcmService
    {
        /// <summary>Send a data-only push message to a specific device token.</summary>
        Task SendToTokenAsync(string token, Dictionary<string, string> data, CancellationToken cancellationToken = default);

        /// <summary>Send a data-only push message to multiple device tokens.</summary>
        Task SendToMultipleTokensAsync(IEnumerable<string> tokens, Dictionary<string, string> data, CancellationToken cancellationToken = default);

        /// <summary>Send a data-only push message to a Firebase topic.</summary>
        Task SendToTopicAsync(string topic, Dictionary<string, string> data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send a data-only push message to a user by their ID.
        /// Fetches the user's FCM tokens from the database, sends the message, and cleans up stale tokens.
        /// </summary>
        Task SendToUserByIdAsync(int userId, Dictionary<string, string> data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send a high-priority, data-only push message for VoIP call notifications.
        /// Does NOT include a Notification object so the OS won't display a banner,
        /// allowing the Flutter client to handle ringing via native CallKit / Agora UI.
        /// </summary>
        Task SendCallNotificationAsync(int userId, Dictionary<string, string> data, CancellationToken cancellationToken = default);
    }
}
