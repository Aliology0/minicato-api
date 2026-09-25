using AlMostashar.Application.Common.Interfaces;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Infrastructure.Services;

public class FcmNotificationService : IFcmService
{
    private readonly IAppDbContext _db;
    private readonly ILogger<FcmNotificationService> _logger;

    /// <summary>
    /// Android notification channel ID. Must match the channel created on the client side
    /// for heads-up / high-importance notifications (Android 8+).
    /// </summary>
    private const string AndroidChannelId = "almostashar_main_channel";

    public FcmNotificationService(IAppDbContext db, ILogger<FcmNotificationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task SendToTokenAsync(string token, Dictionary<string, string> data, CancellationToken cancellationToken = default)
    {
        var message = new Message
        {
            Token = token,
            Notification = BuildNotification(data),
            Data = data,
            Android = BuildAndroidConfig(),
            Apns = new ApnsConfig
            {
                Headers = new Dictionary<string, string> { { "apns-priority", "10" } }
            }
        };

        await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SendToMultipleTokensAsync(IEnumerable<string> tokens, Dictionary<string, string> data, CancellationToken cancellationToken = default)
    {
        var tokenList = tokens.ToList();

        _logger.LogInformation("[FCM] SendToMultipleTokensAsync — TokenCount={Count}, Data={@Data}", tokenList.Count, data);

        var multicast = new MulticastMessage
        {
            Tokens = tokenList,
            Notification = BuildNotification(data),
            Data = data,
            Android = BuildAndroidConfig(),
            Apns = new ApnsConfig
            {
                Headers = new Dictionary<string, string> { { "apns-priority", "10" } }
            }

        };

        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(multicast, cancellationToken);

            _logger.LogInformation(
                "[FCM] FCM response — SuccessCount={Success}, FailureCount={Failure}",
                response.SuccessCount, response.FailureCount);

            // Log individual token results for debugging
            for (int i = 0; i < response.Responses.Count; i++)
            {
                var r = response.Responses[i];
                if (r.IsSuccess)
                {
                    _logger.LogDebug("[FCM] Token[{Index}] ✓ MessageId={MessageId}", i, r.MessageId);
                }
                else
                {
                    _logger.LogWarning(
                        "[FCM] Token[{Index}] ✗ ErrorCode={ErrorCode}, Message={Message}",
                        i, r.Exception?.MessagingErrorCode, r.Exception?.Message);
                }
            }

            // Clean up any stale tokens that Firebase rejected
            await RemoveStaleTokensAsync(tokenList, response.Responses, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[FCM] CRITICAL — Failed to send notification via FCM. TokenCount={Count}", tokenList.Count);
        }
    }

    /// <inheritdoc/>
    public async Task SendToTopicAsync(string topic, Dictionary<string, string> data, CancellationToken cancellationToken = default)
    {
        var message = new Message
        {
            Topic = topic,
            Notification = BuildNotification(data),
            Data = data,
            Android = BuildAndroidConfig()
        };

        await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SendToUserByIdAsync(int userId, Dictionary<string, string> data, CancellationToken cancellationToken = default)
    {
        var tokens = await _db.UserFcmTokens
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .Select(t => t.Token)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("[FCM] SendToUserByIdAsync — UserId={UserId}, TokensFound={Count}", userId, tokens.Count);

        if (tokens.Count > 0)
        {
            await SendToMultipleTokensAsync(tokens, data, cancellationToken);
        }
        else
        {
            _logger.LogWarning("[FCM] No FCM tokens found for UserId={UserId}. Notification will NOT be delivered.", userId);
        }
    }

    /// <inheritdoc/>
    public async Task SendCallNotificationAsync(int userId, Dictionary<string, string> data, CancellationToken cancellationToken = default)
    {
        var tokens = await _db.UserFcmTokens
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .Select(t => t.Token)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "[FCM-CALL] SendCallNotificationAsync — UserId={UserId}, TokensFound={Count}, Data={@Data}",
            userId, tokens.Count, data);

        if (tokens.Count > 0)
        {
            await SendCallToMultipleTokensAsync(tokens, data, cancellationToken);
        }
        else
        {
            _logger.LogWarning("[FCM-CALL] No FCM tokens found for UserId={UserId}. Call notification will NOT be delivered!", userId);
        }
    }

    // ─── Private Helpers ─────────────────────────────────────────────────────

    /// <summary>
    /// Sends a data-only multicast message for VoIP calls.
    /// No <see cref="Notification"/> object is included so the OS won't display
    /// a system notification banner, leaving the ringing UI entirely to the client
    /// (Flutter CallKit / Agora). High priority ensures delivery when the app is killed.
    /// </summary>
    private async Task SendCallToMultipleTokensAsync(IEnumerable<string> tokens, Dictionary<string, string> data, CancellationToken cancellationToken)
    {
        var tokenList = tokens.ToList();

        _logger.LogInformation("[FCM-CALL] Sending data-only call notification to {Count} token(s)", tokenList.Count);

        var multicast = new MulticastMessage
        {
            Tokens = tokenList,
            Data = data,
            Android = new AndroidConfig
            {
                Priority = Priority.High
            },
            Apns = new ApnsConfig
            {
                Headers = new Dictionary<string, string>
                {
                    { "apns-priority", "10" },
                    { "apns-push-type", "background" }
                },
                Aps = new Aps
                {
                    ContentAvailable = true
                }
            }
        };

        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(multicast, cancellationToken);

            _logger.LogInformation(
                "[FCM-CALL] FCM response — SuccessCount={Success}, FailureCount={Failure}",
                response.SuccessCount, response.FailureCount);

            // Log individual token results for debugging
            for (int i = 0; i < response.Responses.Count; i++)
            {
                var r = response.Responses[i];
                if (r.IsSuccess)
                {
                    _logger.LogDebug("[FCM-CALL] Token[{Index}] ✓ MessageId={MessageId}", i, r.MessageId);
                }
                else
                {
                    _logger.LogWarning(
                        "[FCM-CALL] Token[{Index}] ✗ ErrorCode={ErrorCode}, Message={Message}",
                        i, r.Exception?.MessagingErrorCode, r.Exception?.Message);
                }
            }

            await RemoveStaleTokensAsync(tokenList, response.Responses, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[FCM-CALL] CRITICAL — Failed to send call notification via FCM. TokenCount={Count}", tokenList.Count);
        }
    }

    /// <summary>
    /// Builds the FCM <see cref="Notification"/> payload from the data dictionary.
    /// This is essential for Android to display the notification when the app is
    /// killed or removed from recents – data-only messages are silently dropped
    /// in that state because the app's FirebaseMessagingService is not running.
    /// </summary>
    private static Notification BuildNotification(Dictionary<string, string> data)
    {
        data.TryGetValue("title", out var title);
        data.TryGetValue("message", out var body);

        // For incoming-call payloads the title/body keys may not be present;
        // fall back to caller info so the system tray still shows something useful.
        if (string.IsNullOrWhiteSpace(title))
            data.TryGetValue("callerName", out title);

        if (string.IsNullOrWhiteSpace(body) && data.ContainsKey("callSessionId"))
            body = "Incoming call";

        return new Notification
        {
            Title = title ?? "AlMostashar",
            Body = body ?? string.Empty
        };
    }

    /// <summary>
    /// Builds a shared <see cref="AndroidConfig"/> with high priority and the
    /// correct notification channel so the OS shows heads-up notifications.
    /// </summary>
    private static AndroidConfig BuildAndroidConfig()
    {
        return new AndroidConfig
        {
            Priority = Priority.High,
            Notification = new AndroidNotification
            {
                ChannelId = AndroidChannelId,
                // Use default sound so the device rings / vibrates
                DefaultSound = true,
                NotificationCount = 1
            }
        };
    }

    /// <summary>
    /// Deletes FCM tokens that Firebase rejected as invalid or expired.
    /// Keeps the DB clean so we don't keep sending to dead devices.
    /// </summary>
    private async Task RemoveStaleTokensAsync(
        List<string> tokens,
        IReadOnlyList<SendResponse> responses,
        CancellationToken cancellationToken)
    {
        var staleTokens = new List<string>();

        for (int i = 0; i < responses.Count; i++)
        {
            var response = responses[i];
            if (!response.IsSuccess)
            {
                var errorCode = response.Exception?.MessagingErrorCode;
                if (errorCode is MessagingErrorCode.Unregistered or MessagingErrorCode.InvalidArgument)
                {
                    staleTokens.Add(tokens[i]);
                }
            }
        }

        if (staleTokens.Count > 0)
        {
            _logger.LogWarning("[FCM] Removing {Count} stale/invalid FCM token(s) from database", staleTokens.Count);

            var toDelete = await _db.UserFcmTokens
                .Where(t => staleTokens.Contains(t.Token))
                .ToListAsync(cancellationToken);

            _db.UserFcmTokens.RemoveRange(toDelete);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}