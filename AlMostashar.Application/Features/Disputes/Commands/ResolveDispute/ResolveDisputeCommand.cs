using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using System.Text.Json.Serialization;

namespace AlMostashar.Application.Features.Disputes.Commands.ResolveDispute;

public class ResolveDisputeCommand : IRequest<Result<string>>
{
    [JsonIgnore]
    public int DisputeId { get; set; }

    public DisputeResolutionType ResolutionType { get; set; }
    
    public string AdminNotes { get; set; } = string.Empty;
    public string AdminDecision { get; set; } = string.Empty;
}
