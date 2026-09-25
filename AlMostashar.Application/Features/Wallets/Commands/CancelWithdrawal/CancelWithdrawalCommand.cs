using AlMostashar.Application.Features.Wallets.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Wallets.Commands.CancelWithdrawal;

public record CancelWithdrawalCommand(int WithdrawalId) : IRequest<Result<WithdrawalRequestDto>>;
