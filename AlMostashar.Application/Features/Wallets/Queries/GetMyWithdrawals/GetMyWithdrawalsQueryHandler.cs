using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.Wallets.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Wallets.Queries.GetMyWithdrawals;

public class GetMyWithdrawalsQueryHandler
    : IRequestHandler<GetMyWithdrawalsQuery, Result<CursorPagedResult<WithdrawalRequestDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMyWithdrawalsQueryHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<CursorPagedResult<WithdrawalRequestDto>>> Handle(
        GetMyWithdrawalsQuery request,
        CancellationToken cancellationToken)
    {
        var lawyerId = await _db.Lawyers
            .AsNoTracking()
            .Where(l => l.Id == _currentUser.UserId)
            .Select(l => (int?)l.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (!lawyerId.HasValue)
        {
            return Result<CursorPagedResult<WithdrawalRequestDto>>.Failure(
                new Error("Lawyer.Auth.Unauthorized", "Authenticated user is not a lawyer."));
        }

        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var query = _db.WithdrawalRequests
            .AsNoTracking()
            .Where(wr => wr.LawyerId == lawyerId.Value);

        if (request.Cursor.HasValue)
        {
            query = query.Where(wr => wr.Id < request.Cursor.Value);
        }

        var withdrawals = await query
            .OrderByDescending(wr => wr.Id)
            .Take(pageSize + 1)
            .Select(wr => new WithdrawalRequestDto
            {
                Id = wr.Id,
                LawyerId = wr.LawyerId,
                WalletId = wr.WalletId,
                Amount = wr.Amount,
                Method = wr.Method,
                AccountDetails = wr.AccountDetailsMasked,
                AccountDetailsMasked = wr.AccountDetailsMasked,
                Status = wr.Status,
                RequestedAt = wr.RequestedAt,
                ReviewedAt = wr.ReviewedAt,
                ReviewedByAdminId = wr.ReviewedByAdminId,
                AdminNotes = wr.AdminNotes,
                RejectionReason = wr.RejectionReason,
                PayoutReference = wr.PayoutReference,
                PayoutProvider = wr.PayoutProvider,
                PaidByAdminId = wr.PaidByAdminId,
                PaidAt = wr.PaidAt,
                WalletTransactionId = wr.WalletTransactionId
            })
            .ToListAsync(cancellationToken);

        var hasMore = withdrawals.Count > pageSize;
        if (hasMore)
        {
            withdrawals.RemoveAt(withdrawals.Count - 1);
        }

        return Result<CursorPagedResult<WithdrawalRequestDto>>.Success(new CursorPagedResult<WithdrawalRequestDto>
        {
            Items = withdrawals,
            HasMore = hasMore,
            NextCursor = hasMore && withdrawals.Count > 0 ? withdrawals[^1].Id : null
        });
    }
}
