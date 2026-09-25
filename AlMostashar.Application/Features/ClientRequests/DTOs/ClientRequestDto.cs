using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Application.Features.ClientRequests.DTOs;

/// <summary>Client-side request DTO. Counterparty is always the lawyer.</summary>
public class ClientRequestDto
{
    public int Id { get; set; }
    public string RequestId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string ProblemDetails { get; set; } = null!;
    public ClientRequestStatus Status { get; set; }
    public string Type { get; set; } = null!;
    public string? ServiceTitle { get; set; }
    public decimal? Price { get; set; }
    public decimal? Budget { get; set; }
    public int OfferCount { get; set; }
    public List<DocumentDto> Documents { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public int? LegalServiceId { get; set; }
    public ServiceType ServiceType { get; set; }
    public DateTime? ClientDeadline { get; set; }
    public CommunicationMethod? PreferredCommunicationMethod { get; set; }
    public RequestUrgency Urgency { get; set; }
    public object? RequestDetails { get; set; }

    // Lawyer info (null before acceptance)
    public int? LawyerId { get; set; }
    public string? LawyerName { get; set; }
    public string? LawyerProfileImage { get; set; }

    public LocationDto Location { get; set; } = null!;
    public InvoiceDto? Invoice { get; set; }
}

/// <summary>Lawyer-side request DTO. Counterparty is always the client.</summary>
public class LawyerDirectRequestDto
{
    public int Id { get; set; }
    public string RequestId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string ProblemDetails { get; set; } = null!;
    public ClientRequestStatus Status { get; set; }
    public string Type { get; set; } = null!;
    public string? ServiceTitle { get; set; }
    public decimal? Price { get; set; }
    public List<DocumentDto> Documents { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public int? LegalServiceId { get; set; }
    public ServiceType ServiceType { get; set; }
    public DateTime? ClientDeadline { get; set; }
    public CommunicationMethod? PreferredCommunicationMethod { get; set; }
    public RequestUrgency Urgency { get; set; }
    public object? RequestDetails { get; set; }

    // Client info
    public int ClientId { get; set; }
    public string ClientName { get; set; } = null!;
    public string? ClientProfileImage { get; set; }

    public LocationDto Location { get; set; } = null!;
}

public class LocationDto
{
    public int? GovernorateId { get; set; }
    public string Governorate { get; set; } = string.Empty;
    public int? CityId { get; set; }
    public string? City { get; set; }
}

public class InvoiceDto
{
    public int Id { get; set; }
    public InvoiceStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string ReferenceNumber { get; set; } = null!;
    public string? ServiceTitle { get; set; }
}
