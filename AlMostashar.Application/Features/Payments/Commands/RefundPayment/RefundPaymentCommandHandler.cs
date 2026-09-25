using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Models;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Application.Features.Payments.Commands.RefundPayment;

public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, Result<RefundPaymentResultDto>>
{
    private readonly IAppDbContext _db;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<RefundPaymentCommandHandler> _logger;

    public RefundPaymentCommandHandler(
        IAppDbContext db,
        IPaymentService paymentService,
        ILogger<RefundPaymentCommandHandler> logger)
    {
        _db = db;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<Result<RefundPaymentResultDto>> Handle(
        RefundPaymentCommand request,
        CancellationToken cancellationToken)
    {
        // ── Step 1: Load payment ──
        var payment = await _db.Payments
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

        if (payment is null)
            return Result<RefundPaymentResultDto>.Failure(
                new Error("Payment.NotFound", $"Payment with Id={request.PaymentId} was not found."));

        // ── Step 2: Validate payment status ──
        // Allow refund only for Succeeded or Refunded (partial refund scenario)
        if (payment.Status is not (PaymentStatus.Succeeded or PaymentStatus.Refunded))
            return Result<RefundPaymentResultDto>.Failure(
                new Error("Payment.InvalidStatus",
                    $"Cannot refund a payment with status '{payment.Status}'. Only Succeeded or Refunded payments can be refunded."));

        // ── Step 3: Validate Paymob transaction id ──
        if (!payment.TransactionId.HasValue)
            return Result<RefundPaymentResultDto>.Failure(
                new Error("Payment.TransactionIdMissing",
                    "Payment does not have a Paymob TransactionId. Cannot process refund."));

        // ── Step 4: Calculate refundable amount ──
        var alreadyRefundedAmount = payment.RefundedAmount;
        var remainingRefundableAmount = payment.Amount - alreadyRefundedAmount;

        if (payment.Amount <= 0)
            return Result<RefundPaymentResultDto>.Failure(
                new Error("Payment.InvalidRefundState",
                    "Payment amount is zero or negative. Cannot process refund."));

        if (alreadyRefundedAmount < 0 || alreadyRefundedAmount > payment.Amount)
            return Result<RefundPaymentResultDto>.Failure(
                new Error("Payment.InvalidRefundState",
                    $"Payment refund state is inconsistent. RefundedAmount={alreadyRefundedAmount}, Amount={payment.Amount}."));

        if (remainingRefundableAmount <= 0)
            return Result<RefundPaymentResultDto>.Failure(
                new Error("Payment.AlreadyFullyRefunded",
                    "Payment has already been fully refunded. No remaining amount to refund."));

        // ── Step 5: Determine requested refund amount ──
        decimal requestedRefundAmount;

        if (request.RefundedAmount.HasValue)
        {
            requestedRefundAmount = request.RefundedAmount.Value;
        }
        else
        {
            // Full refund of remaining amount
            requestedRefundAmount = remainingRefundableAmount;
        }

        if (requestedRefundAmount <= 0)
            return Result<RefundPaymentResultDto>.Failure(
                new Error("Payment.InvalidRefundAmount",
                    "Refund amount must be greater than zero."));

        if (requestedRefundAmount > remainingRefundableAmount)
            return Result<RefundPaymentResultDto>.Failure(
                new Error("Payment.RefundAmountExceedsRemaining",
                    $"Requested refund amount ({requestedRefundAmount} EGP) exceeds the remaining refundable amount ({remainingRefundableAmount} EGP)."));

        // ── Step 6: Convert amount to cents ──
        var amountCents = decimal.Round(requestedRefundAmount * 100m, 0, MidpointRounding.AwayFromZero);

        if (amountCents <= 0)
            return Result<RefundPaymentResultDto>.Failure(
                new Error("Payment.InvalidRefundAmount",
                    "Calculated refund amount in cents is zero or negative after rounding."));

        // ── Step 7: Call Paymob refund method ──
        RefundPaymentResultDto refundResult;

        try
        {
            refundResult = await _paymentService.RefundPaymentAsync(
                payment.TransactionId.Value,
                amountCents);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Paymob refund failed. PaymentId={PaymentId}, TransactionId={TransactionId}, " +
                "RequestedRefundAmount={RequestedRefundAmount}, AmountCents={AmountCents}, " +
                "AlreadyRefundedAmount={AlreadyRefundedAmount}, RemainingRefundableAmount={RemainingRefundableAmount}",
                payment.Id,
                payment.TransactionId,
                requestedRefundAmount,
                amountCents,
                alreadyRefundedAmount,
                remainingRefundableAmount);

            return Result<RefundPaymentResultDto>.Failure(
                new Error("Payment.RefundFailed", "Paymob refund failed. Please try again."));
        }

        // ── Step 8: Business validation on amount ──
        if (refundResult.AmountCents != amountCents)
        {
            _logger.LogError(
                "Paymob refund amount mismatch. PaymentId={PaymentId}, TransactionId={TransactionId}, " +
                "RequestedRefundAmount={RequestedRefundAmount}, ExpectedAmountCents={ExpectedAmountCents}, " +
                "ActualAmountCents={ActualAmountCents}, RefundTransactionId={RefundTransactionId}, " +
                "RawResponse={RawResponse}",
                payment.Id,
                payment.TransactionId,
                requestedRefundAmount,
                amountCents,
                refundResult.AmountCents,
                refundResult.RefundTransactionId,
                refundResult.RawResponse);

            return Result<RefundPaymentResultDto>.Failure(
                new Error("Payment.RefundAmountMismatch",
                    $"Paymob returned refund amount ({refundResult.AmountCents} cents) does not match the requested amount ({amountCents} cents)."));
        }

        // ── Step 9: Update payment refund fields ──
            payment.Status = PaymentStatus.Refunded;

        payment.GatewayResponse = refundResult.RawResponse;

        _logger.LogInformation(
            "Paymob refund succeeded. PaymentId={PaymentId}, RefundTransactionId={RefundTransactionId}, " +
            "RefundedAmountCents={RefundedAmountCents}, TotalRefundedAmount={TotalRefundedAmount}, " +
            "PaymentAmount={PaymentAmount}, NewStatus={NewStatus}",
            payment.Id,
            refundResult.RefundTransactionId,
            refundResult.AmountCents,
            payment.RefundedAmount,
            payment.Amount,
            payment.Status);

        // ── Step 10: Save changes ──
        await _db.SaveChangesAsync(cancellationToken);

        return Result<RefundPaymentResultDto>.Success(refundResult);
    }
}
