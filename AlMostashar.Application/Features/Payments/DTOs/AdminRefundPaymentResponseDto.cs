namespace AlMostashar.Application.Features.Payments.DTOs;

public sealed class AdminRefundPaymentResponseDto
{
    public string Message { get; set; } = string.Empty;

    public int PaymentId { get; set; }
    public int? EscrowId { get; set; }

    public decimal PaymentAmount { get; set; }
    public decimal RequestedRefundAmount { get; set; }
    public decimal TotalRefundedAmount { get; set; }
    public decimal RemainingRefundableAmount { get; set; }

    public string? EscrowStatus { get; set; }

    public int? OriginalTransactionId { get; set; }
    public int RefundTransactionId { get; set; }

    public bool IsFullRefund { get; set; }
    public bool EscrowUpdated { get; set; }

    public string? GatewayMessage { get; set; }
}
