namespace AlMostashar.Application.Features.ClientRequests.DTOs;
using AlMostashar.Domain.ValueObject.Enum;

public class BroadcastRequestDto
{
    public int Id { get; set; }
    public string RequestId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string ProblemDetails { get; set; } = null!;
    public string Status { get; set; } = null!;
    public int? LegalServiceId { get; set; }
    public string? ServiceTitle { get; set; }
    public int? GovernorateId { get; set; }
    public string Governorate { get; set; } = null!;
    public int? CityId { get; set; }
    public string? City { get; set; }
    public decimal Budget { get; set; }
    public string ClientName { get; set; } = null!;
    public List<DocumentDto> Documents { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public ServiceType ServiceType { get; set; }
    public DateTime? ClientDeadline { get; set; }
    public CommunicationMethod? PreferredCommunicationMethod { get; set; }
    public RequestUrgency Urgency { get; set; }
    public object? RequestDetails { get; set; }
}
