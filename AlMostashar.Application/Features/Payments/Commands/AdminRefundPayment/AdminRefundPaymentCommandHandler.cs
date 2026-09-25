using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Escrows.Commands.RefundEscrow;
using AlMostashar.Application.Features.Payments.Commands.RefundPayment;
using AlMostashar.Application.Features.Payments.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Application.Features.Payments.Commands.AdminRefundPayment;

public class AdminRefundPaymentCommandHandler
    : IRequestHandler<AdminRefundPaymentCommand, Result<AdminRefundPaymentResponseDto>>
{
    private readonly IAppDbContext _db;
    private readonly IMediator _mediator;
    private readonly ILogger<AdminRefundPaymentCommandHandler> _logger;

    public AdminRefundPaymentCommandHandler(
        IAppDbContext db,
        IMediator mediator,
        ILogger<AdminRefundPaymentCommandHandler> logger)
    {
        _db = db;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result<AdminRefundPaymentResponseDto>> Handle(
        AdminRefundPaymentCommand request,
        CancellationToken cancellationToken)
    {
        // ── Step 1: Load payment before refund (existence check only, no business validation) ──
        var payment = await _db.Payments
            .Where(p => p.Id == request.PaymentId)
            .Select(p => new
            {
                p.Id,
                p.Amount,
                p.RefundedAmount,
                p.TransactionId,
                EscrowId = _db.Escrows
                    .Where(e => e.PaymentId == p.Id)
                    .Select(e => (int?)e.Id)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (payment is null)
            return Result<AdminRefundPaymentResponseDto>.Failure(
                new Error("Payment.NotFound", $"Payment with Id={request.PaymentId} was not found."));

        // Capture the requested refund amount for the response
        // (null means full refund — use the remaining refundable amount)
        var requestedRefundAmount = request.RefundedAmount
            ?? (payment.Amount - payment.RefundedAmount);

        // ── Step 2: Call existing RefundPaymentCommand (all validations live there) ──
        var refundResult = await _mediator.Send(
            new RefundPaymentCommand(request.PaymentId, request.RefundedAmount),
            cancellationToken);

        if (!refundResult.IsSuccess)
            return Result<AdminRefundPaymentResponseDto>.Failure(refundResult.Error!);

        var refundedAmount = refundResult.Value!.AmountCents / 100m;
        var TotalRefunded = payment.RefundedAmount + refundedAmount;
        // ── Step 5: Update escrow only on full refund ──
        bool isFullRefund = TotalRefunded >= payment.Amount;
        bool escrowUpdated = false;

        if (isFullRefund && payment.EscrowId.HasValue)
        {
            try
            {
                var refundEscrowResult = await _mediator.Send(
                    new RefundEscrowCommand(payment.EscrowId.Value),
                    cancellationToken);

                if (!refundEscrowResult.IsSuccess)
                {
                    _logger.LogError(
                        "Admin refund succeeded in Paymob but RefundEscrowCommand failed. " +
                        "PaymentId={PaymentId}, EscrowId={EscrowId}, RefundTransactionId={RefundTransactionId}, " +
                        "ErrorCode={ErrorCode}, ErrorMessage={ErrorMessage}, Reason={Reason}",
                        request.PaymentId,
                        payment.EscrowId.Value,
                        refundResult.Value!.RefundTransactionId,
                        refundEscrowResult.Error!.Code,
                        refundEscrowResult.Error.Message,
                        request.Reason);

                    return Result<AdminRefundPaymentResponseDto>.Failure(
                        new Error(
                            "Escrow.RefundStateUpdateFailed",
                            "Payment refund succeeded, but escrow state update failed. Please retry or handle manually."));
                }

                escrowUpdated = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Admin refund succeeded in Paymob but RefundEscrowCommand threw exception. " +
                    "PaymentId={PaymentId}, EscrowId={EscrowId}, RefundTransactionId={RefundTransactionId}, Reason={Reason}",
                    request.PaymentId,
                    payment.EscrowId.Value,
                    refundResult.Value!.RefundTransactionId,
                    request.Reason);

                return Result<AdminRefundPaymentResponseDto>.Failure(
                    new Error(
                        "Escrow.RefundStateUpdateFailed",
                        "Payment refund succeeded, but escrow state update failed. Please retry or handle manually."));
            }

        }
        // ── Step 6: Build response ──
        return Result<AdminRefundPaymentResponseDto>.Success(new AdminRefundPaymentResponseDto
        {
            Message = isFullRefund
                ? "Payment refunded successfully."
                : "Payment partially refunded successfully.",

            PaymentId = payment.Id,
            EscrowId = payment.EscrowId,

            PaymentAmount = payment.Amount,
            RequestedRefundAmount = refundedAmount,
            TotalRefundedAmount = TotalRefunded,
            RemainingRefundableAmount = Math.Max(payment.Amount - TotalRefunded, 0),

            OriginalTransactionId = payment.TransactionId,
            RefundTransactionId = refundResult.Value!.RefundTransactionId,

            IsFullRefund = isFullRefund,
            EscrowUpdated = escrowUpdated,

            GatewayMessage = refundResult.Value.GatewayMessage
        });
    }
}
