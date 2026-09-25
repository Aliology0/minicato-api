using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.Wallets.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Wallets.Queries.GetAdminWithdrawals;

public class GetAdminWithdrawalsQueryHandler
    : IRequestHandler<GetAdminWithdrawalsQuery, Result<CursorPagedResult<WithdrawalRequestDto>>>
{
    private readonly IAppDbContext _db;

    public GetAdminWithdrawalsQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<CursorPagedResult<WithdrawalRequestDto>>> Handle(
        GetAdminWithdrawalsQuery request,
        CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var query = _db.WithdrawalRequests
            .AsNoTracking()
            .AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(wr => wr.Status == request.Status.Value);
        }

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
                WalletTransactionId = wr.WalletTransactionId,
                LawyerName = wr.Lawyer.FullName,
                LawyerEmail = wr.Lawyer.Email
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
