using System.Globalization;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Wallets.DTOs;
using AlMostashar.Application.Features.Wallets.Helpers;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Wallets.Commands.CancelWithdrawal;

public class CancelWithdrawalCommandHandler : IRequestHandler<CancelWithdrawalCommand, Result<WithdrawalRequestDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CancelWithdrawalCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<WithdrawalRequestDto>> Handle(CancelWithdrawalCommand request, CancellationToken cancellationToken)
    {
        Result<WithdrawalRequestDto>? result = null;

        await _db.ExecuteInTransactionAsync(async ct =>
        {
            var withdrawal = await _db.WithdrawalRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(wr => wr.Id == request.WithdrawalId, ct);

            if (withdrawal is null || withdrawal.LawyerId != _currentUser.UserId)
            {
                result = Failure("Withdrawal.NotFound", "Withdrawal request was not found.");
                return;
            }

            if (!withdrawal.CanTransitionTo(WithdrawalStatus.Cancelled))
            {
                result = Failure("Withdrawal.InvalidStatus", $"Only pending withdrawals can be cancelled. Current status is {withdrawal.Status}.");
                return;
            }

            var now = DateTime.UtcNow;
            var transitionRows = await _db.WithdrawalRequests
                .Where(wr => wr.Id == withdrawal.Id &&
                             wr.LawyerId == _currentUser.UserId &&
                             wr.Status == WithdrawalStatus.Pending)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(wr => wr.Status, WithdrawalStatus.Cancelled)
                        .SetProperty(wr => wr.ReviewedAt, now),
                    ct);

            if (transitionRows != 1)
            {
                result = Failure("Withdrawal.StateConflict", "Withdrawal status changed before cancellation could be applied.");
                return;
            }

            WalletBalanceTrackingSync.SyncWithdrawal(_db, withdrawal.Id, wr =>
            {
                wr.Status = WithdrawalStatus.Cancelled;
                wr.ReviewedAt = now;
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
                ReferenceType = "WithdrawalCancellation",
                ReferenceId = withdrawal.Id.ToString(CultureInfo.InvariantCulture),
                WithdrawalRequestId = withdrawal.Id,
                Description = $"Withdrawal request #{withdrawal.Id} cancelled",
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
