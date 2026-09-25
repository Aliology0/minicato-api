using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Domain.Entities;

/// <summary>
/// Tracks a single call (video or voice) between two chat participants.
/// </summary>
public class CallSession : BaseEntity
{
    // ─── Properties ───
    public string ChannelName { get; private set; } = null!;
    public CallType CallType { get; private set; }
    public CallStatus Status { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public int? DurationInSeconds { get; private set; }

    // ─── Foreign Keys ───
    public int ChatId { get; private set; }
    public int CallerId { get; private set; }
    public int ReceiverId { get; private set; }

    // ─── Navigation Properties ───
    // Only Chat has a navigation — Caller/Receiver are plain IDs since
    // calls can only happen within a Chat (authorization goes through ChatParticipants).
    public Chat Chat { get; private set; } = null!;

    // EF Core requires a parameterless constructor
    private CallSession() { }

    /// <summary>
    /// Creates a new call session in the Initiated state.
    /// </summary>
    public static CallSession Create(string channelName, int chatId, int callerId, int receiverId, CallType callType)
    {
        return new CallSession
        {
            ChannelName = channelName,
            ChatId = chatId,
            CallerId = callerId,
            ReceiverId = receiverId,
            CallType = callType,
            Status = CallStatus.Initiated,
            StartTime = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates the call status. Sets EndTime and Duration on terminal states.
    /// Only allows valid transitions (e.g., cannot go from Completed back to Ongoing).
    /// </summary>
    public void UpdateStatus(CallStatus newStatus)
    {
        // Prevent updating a call that is already in a terminal state
        if (Status == CallStatus.Completed || Status == CallStatus.Missed || Status == CallStatus.Rejected)
        {
            throw new InvalidOperationException(
                $"Cannot change status from '{Status}' to '{newStatus}'. The call has already ended.");
        }

        // Validate allowed transitions
        bool isValid = (Status, newStatus) switch
        {
            (CallStatus.Initiated, CallStatus.Ongoing) => true,
            (CallStatus.Initiated, CallStatus.Missed) => true,
            (CallStatus.Initiated, CallStatus.Rejected) => true,
            (CallStatus.Initiated, CallStatus.Completed) => true,
            (CallStatus.Ongoing, CallStatus.Completed) => true,
            _ => false
        };

        if (!isValid)
        {
            throw new InvalidOperationException(
                $"Invalid status transition from '{Status}' to '{newStatus}'.");
        }

        Status = newStatus;

        // Set EndTime and Duration when the call reaches a terminal state
        if (newStatus is CallStatus.Completed or CallStatus.Missed or CallStatus.Rejected)
        {
            EndTime = DateTime.UtcNow;
            DurationInSeconds = (int)(EndTime.Value - StartTime).TotalSeconds;
        }
    }

    /// <summary>
    /// Force-closes a stale call (zombie cleanup). Used by the system, not by users directly.
    /// </summary>
    public void ForceClose()
    {
        Status = CallStatus.Completed;
        EndTime = DateTime.UtcNow;
        DurationInSeconds = (int)(EndTime.Value - StartTime).TotalSeconds;
    }
}
