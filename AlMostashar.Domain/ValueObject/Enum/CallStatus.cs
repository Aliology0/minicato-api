namespace AlMostashar.Domain.ValueObject.Enum
{
    /// <summary>
    /// Represents the lifecycle states of a call session.
    /// </summary>
    public enum CallStatus
    {
        // Call was created, waiting for the receiver to pick up
        Initiated = 0,

        // Call is currently in progress
        Ongoing = 1,

        // Call ended normally by either participant
        Completed = 2,

        // Receiver did not pick up in time
        Missed = 3,

        // Receiver actively declined the call
        Rejected = 4
    }
}
