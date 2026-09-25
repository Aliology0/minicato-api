using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.Wallets.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Wallets.Queries.GetMyWithdrawals;

public record GetMyWithdrawalsQuery(
    int? Cursor = null,
    int PageSize = 20)
    : IRequest<Result<CursorPagedResult<WithdrawalRequestDto>>>;
