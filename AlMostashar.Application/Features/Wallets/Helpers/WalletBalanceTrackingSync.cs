using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Wallets.Helpers;

internal static class WalletBalanceTrackingSync
{
    public static void SyncAvailableBalance(IAppDbContext db, int walletId, decimal availableBalance)
    {
        if (db is not DbContext dbContext)
        {
            return;
        }

        var trackedWallet = dbContext.ChangeTracker
            .Entries<Wallet>()
            .FirstOrDefault(e => e.Entity.Id == walletId);

        if (trackedWallet is null)
        {
            return;
        }

        trackedWallet.Entity.AvailableBalance = availableBalance;
        trackedWallet.State = EntityState.Unchanged;
    }

    public static void SyncBalances(IAppDbContext db, int walletId, decimal availableBalance, decimal total)
    {
        if (db is not DbContext dbContext)
        {
            return;
        }

        var trackedWallet = dbContext.ChangeTracker
            .Entries<Wallet>()
            .FirstOrDefault(e => e.Entity.Id == walletId);

        if (trackedWallet is null)
        {
            return;
        }

        trackedWallet.Entity.AvailableBalance = availableBalance;
        trackedWallet.Entity.Total = total;
        trackedWallet.State = EntityState.Unchanged;
    }

    public static void SyncWithdrawal(
        IAppDbContext db,
        int withdrawalId,
        Action<WithdrawalRequest> applyChanges)
    {
        if (db is not DbContext dbContext)
        {
            return;
        }

        var trackedWithdrawal = dbContext.ChangeTracker
            .Entries<WithdrawalRequest>()
            .FirstOrDefault(e => e.Entity.Id == withdrawalId);

        if (trackedWithdrawal is null)
        {
            return;
        }

        applyChanges(trackedWithdrawal.Entity);
        trackedWithdrawal.State = EntityState.Unchanged;
    }
}
