using System.Globalization;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Wallets.DTOs;
using AlMostashar.Application.Features.Wallets.Helpers;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Wallets.Commands.RejectWithdrawal;

public class RejectWithdrawalCommandHandler : IRequestHandler<RejectWithdrawalCommand, Result<WithdrawalRequestDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RejectWithdrawalCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<WithdrawalRequestDto>> Handle(RejectWithdrawalCommand request, CancellationToken cancellationToken)
    {
        Result<WithdrawalRequestDto>? result = null;

        await _db.ExecuteInTransactionAsync(async ct =>
        {
            var withdrawal = await _db.WithdrawalRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(wr => wr.Id == request.WithdrawalId, ct);

            if (withdrawal is null)
            {
                result = Failure("Withdrawal.NotFound", "Withdrawal request was not found.");
                return;
            }

            if (!withdrawal.CanTransitionTo(WithdrawalStatus.Rejected))
            {
                result = Failure("Withdrawal.InvalidStatus", $"Only pending or approved withdrawals can be rejected before payment. Current status is {withdrawal.Status}.");
                return;
            }

            var now = DateTime.UtcNow;
            var rejectionReason = request.RejectionReason.Trim();
            var transitionRows = await _db.WithdrawalRequests
                .Where(wr => wr.Id == withdrawal.Id &&
                             (wr.Status == WithdrawalStatus.Pending ||
                              wr.Status == WithdrawalStatus.Approved))
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(wr => wr.Status, WithdrawalStatus.Rejected)
                        .SetProperty(wr => wr.ReviewedAt, now)
                        .SetProperty(wr => wr.ReviewedByAdminId, _currentUser.UserId)
                        .SetProperty(wr => wr.AdminNotes, request.AdminNotes)
                        .SetProperty(wr => wr.RejectionReason, rejectionReason),
                    ct);

            if (transitionRows != 1)
            {
                result = Failure("Withdrawal.StateConflict", "Withdrawal status changed before rejection could be applied.");
                return;
            }

            WalletBalanceTrackingSync.SyncWithdrawal(_db, withdrawal.Id, wr =>
            {
                wr.Status = WithdrawalStatus.Rejected;
                wr.ReviewedAt = now;
                wr.ReviewedByAdminId = _currentUser.UserId;
                wr.AdminNotes = request.AdminNotes;
                wr.RejectionReason = rejectionReason;
            });

            await RestoreWalletBalanceAsync(withdrawal.WalletId, withdrawal.Amount, ct);

            var balanceAfter = await _db.Wallets
                .AsNoTracking()
                .Where(w => w.Id == withdrawal.WalletId)
                .Select(w => w.AvailableBalance)
                .SingleAsync(ct);

            WalletBalanceTrackingSync.SyncAvailableBalance(_db, withdrawal.WalletId, balanceAfter);

            _db.WalletTransactions.Add(new WalletTransaction
            {
                WalletId = withdrawal.WalletId,
                Amount = withdrawal.Amount,
                Type = TransactionType.Credit,
                ReferenceType = "WithdrawalRejection",
                ReferenceId = withdrawal.Id.ToString(CultureInfo.InvariantCulture),
                WithdrawalRequestId = withdrawal.Id,
                Description = $"Withdrawal request #{withdrawal.Id} rejected",
                BalanceAfter = balanceAfter,
                CreatedAt = now
            });

            await _db.SaveChangesAsync(ct);

            var updatedWithdrawal = await _db.WithdrawalRequests
                .AsNoTracking()
                .SingleAsync(wr => wr.Id == withdrawal.Id, ct);

            result = Result<WithdrawalRequestDto>.Success(WithdrawalRequestDtoMapper.ToDto(updatedWithdrawal));
        }, cancellationToken);

        return result!;
    }

    private async Task RestoreWalletBalanceAsync(int walletId, decimal amount, CancellationToken cancellationToken)
    {
        var restoreRows = await _db.Wallets
            .Where(w => w.Id == walletId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    w => w.AvailableBalance,
                    w => w.AvailableBalance + amount),
                cancellationToken);

        if (restoreRows != 1)
        {
            throw new InvalidOperationException($"Wallet {walletId} was not found while restoring withdrawal balance.");
        }
    }

    private static Result<WithdrawalRequestDto> Failure(string code, string message)
    {
        return Result<WithdrawalRequestDto>.Failure(new Error(code, message));
    }
}
