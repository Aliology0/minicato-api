using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.Wallets.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Wallets.Queries.GetMyWalletTransactions;

public record GetMyWalletTransactionsQuery(
    int? Cursor = null,
    int PageSize = 20)
    : IRequest<Result<CursorPagedResult<WalletTransactionDto>>>;
