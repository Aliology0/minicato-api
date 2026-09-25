using System.Globalization;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Escrows.DTOs;
using AlMostashar.Application.Features.Wallets.Helpers;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Application.Features.Escrows.Services;

public sealed class EscrowSettlementService : IEscrowSettlementService
{
    private const string EscrowReferenceType = "Escrow";
    private readonly IAppDbContext _db;
    private readonly ILogger<EscrowSettlementService> _logger;

    public EscrowSettlementService(IAppDbContext db, ILogger<EscrowSettlementService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Result<EscrowSettlementResultDto>> ReleaseToLawyerAsync(
        int escrowId,
        bool allowDisputedEscrow,
        CancellationToken cancellationToken)
    {
        var escrow = await _db.Escrows
            .AsNoTracking()
            .Include(e => e.Request)
            .FirstOrDefaultAsync(e => e.Id == escrowId, cancellationToken);

        if (escrow is null)
        {
            return Failure("Escrow.NotFound", "Escrow was not found.");
        }

        if (escrow.Status != EscrowStatus.Funded &&
            !(allowDisputedEscrow && escrow.Status == EscrowStatus.Disputed))
        {
            return Failure("Escrow.InvalidStatus", $"Only funded escrows can be released. Current status is {escrow.Status}.");
        }

        var lawyerId = escrow.Request?.LawyerServiceLawyerId;
        if (!lawyerId.HasValue)
        {
            return Failure("Escrow.LawyerNotAssigned", "The related request does not have an assigned lawyer.");
        }

        var lawyerExists = await _db.Lawyers
            .AnyAsync(l => l.Id == lawyerId.Value, cancellationToken);

        if (!lawyerExists)
        {
            return Failure("Lawyer.NotFound", "The assigned lawyer was not found.");
        }

        var invoice = await _db.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.ClientRequestId == escrow.RequestId, cancellationToken);

        if (invoice is null)
        {
            return Failure("Invoice.NotFound", "Invoice was not found for the escrow request.");
        }

        if (invoice.Status != InvoiceStatus.Paid)
        {
            return Failure("Invoice.InvalidStatus", $"Only paid invoices can be released. Current status is {invoice.Status}.");
        }

        var releaseAmount = invoice.LawyerAmount;
        if (releaseAmount <= 0)
        {
            return Failure("Escrow.InvalidReleaseAmount", "Invoice lawyer amount must be greater than zero.");
        }

        if (releaseAmount > escrow.Amount)
        {
            return Failure("Escrow.InvalidReleaseAmount", "Invoice lawyer amount cannot exceed the held escrow amount.");
        }

        var referenceId = escrow.Id.ToString(CultureInfo.InvariantCulture);
        var existingWalletCredit = await _db.WalletTransactions
            .AnyAsync(
                wt => (wt.EscrowId == escrow.Id ||
                       (wt.ReferenceType == EscrowReferenceType && wt.ReferenceId == referenceId))
                      && wt.Type == TransactionType.Credit,
                cancellationToken);

        if (existingWalletCredit)
        {
            return Failure("Escrow.ReleaseConflict", "A wallet credit already exists for this escrow.");
        }

        var walletInfo = await _db.Wallets
            .AsNoTracking()
            .Where(w => w.LawyerId == lawyerId.Value)
            .Select(w => new { w.Id })
            .FirstOrDefaultAsync(cancellationToken);

        var walletId = walletInfo?.Id;
        if (!walletId.HasValue)
        {
            var newWallet = new Wallet
            {
                LawyerId = lawyerId.Value,
                AvailableBalance = 0,
                Total = 0
            };

            _db.Wallets.Add(newWallet);
            await _db.SaveChangesAsync(cancellationToken);
            walletId = newWallet.Id;
        }

        var now = DateTime.UtcNow;
        var releaseStatusRows = await _db.Escrows
            .Where(e => e.Id == escrow.Id &&
                        (e.Status == EscrowStatus.Funded ||
                         (allowDisputedEscrow && e.Status == EscrowStatus.Disputed)))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(e => e.Status, EscrowStatus.Released)
                    .SetProperty(e => e.EscrowReleasedAt, now),
                cancellationToken);

        if (releaseStatusRows != 1)
        {
            return Failure("Escrow.ReleaseConflict", "Escrow release could not be applied because its state changed.");
        }

        SyncTrackedEscrowRelease(escrow.Id, now);

        var walletUpdateRows = await _db.Wallets
            .Where(w => w.Id == walletId.Value)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(w => w.AvailableBalance, w => w.AvailableBalance + releaseAmount)
                    .SetProperty(w => w.Total, w => w.Total + releaseAmount),
                cancellationToken);

        if (walletUpdateRows != 1)
        {
            throw new InvalidOperationException($"Wallet {walletId.Value} was not found while releasing escrow {escrow.Id}.");
        }

        var walletBalances = await _db.Wallets
            .AsNoTracking()
            .Where(w => w.Id == walletId.Value)
            .Select(w => new { w.AvailableBalance, w.Total })
            .SingleAsync(cancellationToken);

        WalletBalanceTrackingSync.SyncBalances(
            _db,
            walletId.Value,
            walletBalances.AvailableBalance,
            walletBalances.Total);

        var caseId = await _db.CaseClientRequests
            .Where(ccr => ccr.ClientRequestId == escrow.RequestId)
            .Select(ccr => (int?)ccr.CaseId)
            .FirstOrDefaultAsync(cancellationToken);

        var walletTransaction = new WalletTransaction
        {
            WalletId = walletId.Value,
            Amount = releaseAmount,
            Type = TransactionType.Credit,
            ReferenceType = EscrowReferenceType,
            ReferenceId = referenceId,
            EscrowId = escrow.Id,
            InvoiceId = invoice.Id,
            CaseId = caseId,
            Description = $"Escrow release for request {escrow.RequestId}",
            BalanceAfter = walletBalances.AvailableBalance,
            CreatedAt = now
        };

        _db.WalletTransactions.Add(walletTransaction);

        if (escrow.PaymentId.HasValue)
        {
            _db.PaymentWalletTransactions.Add(new PaymentWalletTransaction
            {
                PaymentId = escrow.PaymentId.Value,
                WalletTransaction = walletTransaction,
                CreatedAt = now
            });
        }
        else
        {
            _logger.LogInformation(
                "Escrow {EscrowId} released without linked payment; PaymentWalletTransaction was not created.",
                escrow.Id);
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsReleaseConflict(ex))
        {
            throw new EscrowReleaseConflictException("A wallet credit already exists for this escrow.", ex);
        }

        return Result<EscrowSettlementResultDto>.Success(new EscrowSettlementResultDto
        {
            EscrowId = escrow.Id,
            RequestId = escrow.RequestId,
            LawyerId = lawyerId.Value,
            WalletId = walletId.Value,
            WalletTransactionId = walletTransaction.Id,
            PaymentId = escrow.PaymentId,
            InvoiceId = invoice.Id,
            CaseId = caseId,
            EscrowAmount = escrow.Amount,
            ReleasedAmount = releaseAmount
        });
    }

    private static Result<EscrowSettlementResultDto> Failure(string code, string message)
    {
        return Result<EscrowSettlementResultDto>.Failure(new Error(code, message));
    }

    private static bool IsReleaseConflict(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message ?? exception.Message;
        return message.Contains("IX_WalletTransactions_EscrowId", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase);
    }

    private void SyncTrackedEscrowRelease(int escrowId, DateTime releasedAt)
    {
        if (_db is not DbContext dbContext)
        {
            return;
        }

        var trackedEscrow = dbContext.ChangeTracker
            .Entries<Escrow>()
            .FirstOrDefault(e => e.Entity.Id == escrowId);

        if (trackedEscrow is null)
        {
            return;
        }

        trackedEscrow.Entity.Status = EscrowStatus.Released;
        trackedEscrow.Entity.EscrowReleasedAt = releasedAt;
        trackedEscrow.State = EntityState.Unchanged;
    }
}
