using AlMostashar.Application.Features.Wallets.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Wallets.Queries.GetAdminWithdrawalById;

public record GetAdminWithdrawalByIdQuery(int WithdrawalId) : IRequest<Result<WithdrawalRequestDto>>;
