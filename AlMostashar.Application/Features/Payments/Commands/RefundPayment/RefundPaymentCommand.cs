using AlMostashar.Application.Common.Models;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Payments.Commands.RefundPayment;

/// <summary>
/// Requests a Paymob refund for a given payment.
/// If RefundedAmount is null, a full refund of the remaining refundable amount is performed.
/// If RefundedAmount has a value (in EGP, not cents), a partial refund of that amount is performed.
/// </summary>
public sealed record RefundPaymentCommand(
    int PaymentId,
    decimal? RefundedAmount = null
) : IRequest<Result<RefundPaymentResultDto>>;
