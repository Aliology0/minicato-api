namespace AlMostashar.Application.Common.Models
{
    public sealed class RefundPaymentResultDto
    {
        public int RefundTransactionId { get; set; }
        public int OriginalTransactionId { get; set; }
        public int OrderId { get; set; }

        public decimal AmountCents { get; set; }

        public bool Success { get; set; }
        public bool Pending { get; set; }
        public bool IsRefund { get; set; }
        public bool ErrorOccured { get; set; }

        public string? GatewayMessage { get; set; }
        public string RawResponse { get; set; } = string.Empty;
    }
}
