using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.Wallets.DTOs;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;

namespace AlMostashar.Application.Features.Wallets.Queries.GetAdminWithdrawals;

public record GetAdminWithdrawalsQuery(
    WithdrawalStatus? Status = null,
    int? Cursor = null,
    int PageSize = 20)
    : IRequest<Result<CursorPagedResult<WithdrawalRequestDto>>>;
