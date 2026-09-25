namespace AlMostashar.Application.Features.ClientRequests.DTOs;
using AlMostashar.Domain.ValueObject.Enum;

public class CreateRequestResponseDto
{
    public int Id { get; set; }
    public string RequestId { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int LegalServiceId { get; set; }
    public ServiceType ServiceType { get; set; }
    public DateTime? ClientDeadline { get; set; }
    public CommunicationMethod? PreferredCommunicationMethod { get; set; }
    public RequestUrgency Urgency { get; set; }
    public object? RequestDetails { get; set; }
}
