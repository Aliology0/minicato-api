using AlMostashar.Application.Features.ClientRequests.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using AlMostashar.Domain.ValueObject.Enum;
using System.Text.Json;

namespace AlMostashar.Application.Features.ClientRequests.Commands.CreateDirectRequest;

public class CreateDirectRequestCommand : IRequest<Result<CreateRequestResponseDto>>
{
    public int LawyerId { get; set; }
    public int LegalServiceId { get; set; }
    public string Title { get; set; } = null!;
    public string ProblemDetails { get; set; } = null!;
    public int GovernorateId { get; set; }
    public int CityId { get; set; }
    public DateTime? ClientDeadline { get; set; }
    public CommunicationMethod? PreferredCommunicationMethod { get; set; }
    public RequestUrgency Urgency { get; set; } = RequestUrgency.Normal;
    public JsonElement RequestDetails { get; set; }
    public List<int>? AttachmentIds { get; set; }
}
