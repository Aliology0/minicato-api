using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Application.Features.Escrows.Services;

/// <summary>
/// Idempotent escrow funding service.
/// Contains escrow funding business rules only — no Paymob validation, no wallet operations.
/// No transaction boundary: the caller is responsible for wrapping in a transaction if needed.
/// Can be called by:
///   1. FundEscrowOnInvoicePaidEventHandler
///   2. Webhook duplicate/recovery path
/// </summary>
public sealed class EscrowFundingService : IEscrowFundingService
{
    private readonly IAppDbContext _db;
    private readonly ILogger<EscrowFundingService> _logger;

    public EscrowFundingService(IAppDbContext db, ILogger<EscrowFundingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task FundEscrowAsync(int clientRequestId, int paymentId, decimal paidAmount, CancellationToken cancellationToken)
    {
        // Rule 1: PaidAmount must be positive
        if (paidAmount <= 0)
        {
            _logger.LogError("EscrowFunding: PaidAmount {PaidAmount} is <= 0 for ClientRequestId={ClientRequestId}, PaymentId={PaymentId}. No mutation.",
                paidAmount, clientRequestId, paymentId);
            return;
        }

        var escrow = await _db.Escrows
            .FirstOrDefaultAsync(e => e.RequestId == clientRequestId, cancellationToken);

        if (escrow is null)
        {
            // Rule 2: No escrow exists → create one as Funded
            _logger.LogInformation("EscrowFunding: Creating new Funded escrow for ClientRequestId={ClientRequestId}, PaymentId={PaymentId}, PaidAmount={PaidAmount}.",
                clientRequestId, paymentId, paidAmount);

            escrow = new Escrow
            {
                RequestId = clientRequestId,
                PaymentId = paymentId,
                Amount = paidAmount,
                Status = EscrowStatus.Funded,
                EscrowFundedAt = DateTime.UtcNow
            };

            _db.Escrows.Add(escrow);
            await _db.SaveChangesAsync(cancellationToken);
            return;
        }

        switch (escrow.Status)
        {
            case EscrowStatus.NotFunded:
                // Rule 3: Existing NotFunded → fund it
                _logger.LogInformation("EscrowFunding: Funding existing NotFunded escrow Id={EscrowId} for ClientRequestId={ClientRequestId}, PaymentId={PaymentId}, PaidAmount={PaidAmount}.",
                    escrow.Id, clientRequestId, paymentId, paidAmount);

                escrow.PaymentId = paymentId;
                escrow.Amount = paidAmount;
                escrow.Status = EscrowStatus.Funded;
                escrow.EscrowFundedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync(cancellationToken);
                return;

            case EscrowStatus.Funded:
                // Rule 4: Same amount and same PaymentId → idempotent no-op
                if (escrow.Amount == paidAmount && escrow.PaymentId == paymentId)
                {
                    _logger.LogInformation("EscrowFunding: Idempotent no-op. Escrow Id={EscrowId} already Funded with same Amount={Amount} and PaymentId={PaymentId}.",
                        escrow.Id, paidAmount, paymentId);
                    return;
                }

                // Rule 5: Same amount, PaymentId is null (legacy) → set PaymentId
                if (escrow.Amount == paidAmount && escrow.PaymentId is null)
                {
                    _logger.LogInformation("EscrowFunding: Legacy escrow Id={EscrowId} has null PaymentId. Setting PaymentId={PaymentId}.",
                        escrow.Id, paymentId);

                    escrow.PaymentId = paymentId;
                    await _db.SaveChangesAsync(cancellationToken);
                    return;
                }

                // Rule 6: Different amount or different non-null PaymentId → log error, no mutation
                _logger.LogError("EscrowFunding: Conflict for Escrow Id={EscrowId}. Existing Amount={ExistingAmount}, PaymentId={ExistingPaymentId}. Incoming Amount={IncomingAmount}, PaymentId={IncomingPaymentId}. No mutation.",
                    escrow.Id, escrow.Amount, escrow.PaymentId, paidAmount, paymentId);
                return;

            case EscrowStatus.Released:
            case EscrowStatus.Refunded:
            case EscrowStatus.Disputed:
                // Rule 7: Terminal states → log warning, no mutation
                _logger.LogWarning("EscrowFunding: Escrow Id={EscrowId} is in terminal state {Status}. Cannot fund. No mutation.",
                    escrow.Id, escrow.Status);
                return;

            default:
                _logger.LogError("EscrowFunding: Unexpected escrow status {Status} for Escrow Id={EscrowId}. No mutation.",
                    escrow.Status, escrow.Id);
                return;
        }
    }
}
