using AlMostashar.Application.Features.Wallets.DTOs;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;

namespace AlMostashar.Application.Features.Wallets.Commands.RequestWithdrawal;

public class RequestWithdrawalCommand : IRequest<Result<WithdrawalRequestDto>>
{
    public decimal Amount { get; set; }
    public WithdrawalMethod Method { get; set; }
    public string AccountDetails { get; set; } = string.Empty;
}
