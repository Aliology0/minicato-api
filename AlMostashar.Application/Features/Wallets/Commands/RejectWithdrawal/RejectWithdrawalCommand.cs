using System.Text.Json.Serialization;
using AlMostashar.Application.Features.Wallets.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Wallets.Commands.RejectWithdrawal;

public class RejectWithdrawalCommand : IRequest<Result<WithdrawalRequestDto>>
{
    [JsonIgnore]
    public int WithdrawalId { get; set; }

    public string RejectionReason { get; set; } = string.Empty;
    public string? AdminNotes { get; set; }
}
