namespace AlMostashar.Domain.ValueObject.Enum
{
    public enum PaymentStatus
    {
        // The payment process has started but not yet finished
        Pending = 0,

        // The money has been successfully charged
        Succeeded = 1,

        // The payment was declined by the bank or gateway
        Failed = 2,

        // The user closed the payment page or the session timed out
        Cancelled = 3,

        // The money was returned to the client after a successful payment
        Refunded = 4
    }
}
