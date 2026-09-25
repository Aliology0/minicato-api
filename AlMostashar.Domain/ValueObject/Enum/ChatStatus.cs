namespace AlMostashar.Domain.ValueObject.Enum
{
    public enum ChatStatus
    {
        // The chat is waiting for the lawyer to accept or start
        Pending = 0,

        // The chat is currently open and messages can be sent
        Active = 1,

        // The chat is closed after the consultation or task is finished
        Closed = 2,

        // The chat is temporarily stopped due to a dispute or admin review
        Suspended = 3,

        // The chat is hidden from active lists but kept for history
        Archived = 4
    }
}
