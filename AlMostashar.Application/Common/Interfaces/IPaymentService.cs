using AlMostashar.Application.Common.Models;

namespace AlMostashar.Application.Common.Interfaces
{
    public interface IPaymentService
    {
        /// <summary>
        /// Creates a Paymob payment intention for an existing Payment record.
        /// </summary>
        /// <param name="paymentId">The local Payment.Id — sent as special_reference to Paymob.</param>
        Task<PaymentResultDto> CreatePaymentIntentionAsync(
            int paymentId,
            decimal amountInEgp,
            int? expirationMinutes,
            string itemName,
            string itemDescription,
            string? clientFirstName,
            string? clientLastName,
            string? clientEmail,
            string? clientPhone);

        Task<RefundPaymentResultDto> RefundPaymentAsync( int transactionId, decimal amountCents);
    }

}
