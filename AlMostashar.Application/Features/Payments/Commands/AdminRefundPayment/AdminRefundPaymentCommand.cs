using AlMostashar.Application.Features.Payments.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Payments.Commands.AdminRefundPayment;

/// <summary>
/// Admin-initiated refund for a payment. Orchestrates RefundPaymentCommand and optionally RefundEscrowCommand.
/// RefundedAmount is in EGP. If null, the full remaining refundable amount is refunded.
/// </summary>
public sealed record AdminRefundPaymentCommand(
    int PaymentId,
    decimal? RefundedAmount = null,
    string? Reason = null
) : IRequest<Result<AdminRefundPaymentResponseDto>>;
