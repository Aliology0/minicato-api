using AlMostashar.Domain.Entities.Common;

namespace AlMostashar.Domain.Events;

/// <summary>
/// Raised when an invoice is paid. The Cases feature reacts to auto-create a Case.
/// The Escrow feature reacts to fund an escrow.
/// </summary>
public sealed class InvoicePaidEvent : BaseEvent
{
    public int InvoiceId { get; }
    public int ClientRequestId { get; }
    public int PaymentId { get; }
    public int TransactionId { get; }
    public int PaymobOrderId { get; }

    /// <summary>
    /// The gross amount paid by the client (matches Payment.Amount / Invoice.TotalAmount).
    /// This is NOT the Paymob net amount or the lawyer payout amount.
    /// </summary>
    public decimal PaidAmount { get; }

    public InvoicePaidEvent(int invoiceId, int clientRequestId, int paymentId, int transactionId, int paymobOrderId, decimal paidAmount)
    {
        InvoiceId = invoiceId;
        ClientRequestId = clientRequestId;
        PaymentId = paymentId;
        TransactionId = transactionId;
        PaymobOrderId = paymobOrderId;
        PaidAmount = paidAmount;
    }
}
