using System.Text.Json.Serialization;
using AlMostashar.Application.Features.Wallets.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Wallets.Commands.ApproveWithdrawal;

public class ApproveWithdrawalCommand : IRequest<Result<WithdrawalRequestDto>>
{
    [JsonIgnore]
    public int WithdrawalId { get; set; }

    public string? AdminNotes { get; set; }
}
