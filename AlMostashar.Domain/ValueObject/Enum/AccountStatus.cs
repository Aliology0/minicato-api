namespace AlMostashar.Domain.ValueObject.Enum
{
    public enum AccountStatus
    {
        Active,
        PendingReview,
        EmailVerificationRequired,
        Suspended,
        Verified,
        Rejected,
        PendingReverification,
        PendingChangesReview,
    }
}
