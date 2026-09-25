using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Wallets.DTOs;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AlMostashar.Application.Common.Helpers;

namespace AlMostashar.Application.Features.Wallets.Queries.GetMyWallet;

public class GetMyWalletQueryHandler : IRequestHandler<GetMyWalletQuery, Result<WalletSummaryDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMyWalletQueryHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<WalletSummaryDto>> Handle(GetMyWalletQuery request, CancellationToken cancellationToken)
    {
        var lawyerId = await _db.Lawyers
            .AsNoTracking()
            .Where(l => l.Id == _currentUser.UserId)
            .Select(l => (int?)l.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (!lawyerId.HasValue)
            return Result<WalletSummaryDto>.Failure(new Error("Lawyer.Auth.Unauthorized", "Authenticated user is not a lawyer."));

        var wallet = await _db.Wallets
            .AsNoTracking()
            .Where(w => w.LawyerId == lawyerId.Value)
            .Select(w => new
            {
                w.Id,
                w.AvailableBalance,
                w.Total
            })
            .FirstOrDefaultAsync(cancellationToken);

        var escrowBalance = await _db.Escrows
            .AsNoTracking()
            .Where(e => e.Status == EscrowStatus.Funded &&
                        e.Request.LawyerServiceLawyerId == lawyerId.Value)
            .SumAmountSafeAsync(_db, cancellationToken);

        var pendingWithdrawalBalance = 0m;
        var paidWithdrawalBalance = 0m;
        if (wallet is not null)
        {
            var pendingWithdrawalsQuery = _db.WithdrawalRequests
                .AsNoTracking()
                .Where(wr => wr.WalletId == wallet.Id &&
                             (wr.Status == WithdrawalStatus.Pending ||
                              wr.Status == WithdrawalStatus.Approved));

            var paidWithdrawalsQuery = _db.WithdrawalRequests
                .AsNoTracking()
                .Where(wr => wr.WalletId == wallet.Id &&
                             wr.Status == WithdrawalStatus.Paid);

            if (_db is DbContext efContext &&
                efContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                var amounts = await pendingWithdrawalsQuery
                    .Select(wr => wr.Amount)
                    .ToListAsync(cancellationToken);

                pendingWithdrawalBalance = amounts.Sum();

                var paidAmounts = await paidWithdrawalsQuery
                    .Select(wr => wr.Amount)
                    .ToListAsync(cancellationToken);

                paidWithdrawalBalance = paidAmounts.Sum();
            }
            else
            {
                pendingWithdrawalBalance = await pendingWithdrawalsQuery
                    .SumAsync(wr => (decimal?)wr.Amount, cancellationToken) ?? 0m;

                paidWithdrawalBalance = await paidWithdrawalsQuery
                    .SumAsync(wr => (decimal?)wr.Amount, cancellationToken) ?? 0m;
            }
        }

        return Result<WalletSummaryDto>.Success(new WalletSummaryDto
        {
            AvailableBalance = wallet?.AvailableBalance ?? 0m,
            TotalEarnings = wallet?.Total ?? 0m,
            EscrowBalance = escrowBalance,
            PendingWithdrawalBalance = pendingWithdrawalBalance,
            PaidWithdrawalBalance = paidWithdrawalBalance,
            TotalWithdrawn = paidWithdrawalBalance,
            PendingBalance = pendingWithdrawalBalance
        });
    }
}
