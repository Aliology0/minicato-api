using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.Wallets.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Wallets.Queries.GetMyWalletTransactions;

public class GetMyWalletTransactionsQueryHandler
    : IRequestHandler<GetMyWalletTransactionsQuery, Result<CursorPagedResult<WalletTransactionDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMyWalletTransactionsQueryHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<CursorPagedResult<WalletTransactionDto>>> Handle(
        GetMyWalletTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var lawyerId = await _db.Lawyers
            .AsNoTracking()
            .Where(l => l.Id == _currentUser.UserId)
            .Select(l => (int?)l.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (!lawyerId.HasValue)
            return Result<CursorPagedResult<WalletTransactionDto>>.Failure(new Error("Lawyer.Auth.Unauthorized", "Authenticated user is not a lawyer."));

        var pageSize = Math.Clamp(request.PageSize, 1, 50);

        var query = _db.WalletTransactions
            .AsNoTracking()
            .Where(t => t.Wallet.LawyerId == lawyerId.Value);

        if (request.Cursor.HasValue)
            query = query.Where(t => t.Id < request.Cursor.Value);

        var transactions = await query
            .OrderByDescending(t => t.Id)
            .Take(pageSize + 1)
            .Select(t => new WalletTransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                Type = t.Type,
                ReferenceType = t.ReferenceType,
                ReferenceId = t.ReferenceId,
                EscrowId = t.EscrowId,
                Description = t.Description,
                CreatedAt = t.CreatedAt,
                BalanceAfter = t.BalanceAfter,
                CaseId = t.CaseId,
                InvoiceId = t.InvoiceId,
                WithdrawalRequestId = t.WithdrawalRequestId
            })
            .ToListAsync(cancellationToken);

        var hasMore = transactions.Count > pageSize;
        if (hasMore)
            transactions.RemoveAt(transactions.Count - 1);

        return Result<CursorPagedResult<WalletTransactionDto>>.Success(new CursorPagedResult<WalletTransactionDto>
        {
            Items = transactions,
            HasMore = hasMore,
            NextCursor = hasMore && transactions.Count > 0 ? transactions[^1].Id : null
        });
    }
}
