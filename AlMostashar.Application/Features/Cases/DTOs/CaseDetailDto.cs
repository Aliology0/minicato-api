using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Application.Features.Cases.DTOs;

public class CaseDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ServiceType ServiceType { get; set; }
    public CaseStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public CaseSource Source { get; set; }
    public string? ClientName { get; set; }
    public string? LawyerName { get; set; }
    public string? Reference { get; set; }
    public int LawyerId { get; set; }
    public int? ClientRequestId { get; set; }
    public string? ClientRequestReference { get; set; }
    public int? ServiceId { get; set; }
    public int? ChatId { get; set; }
    public string? CancellationReason { get; set; }

    // Subtype-specific fields (nullable — only populated for the matching service type)
    public DateTime? AppointmentDate { get; set; }
    public CommunicationMethod? CommunicationMethod { get; set; }
    public string? LegalBranch { get; set; }
    public ContractType? ContractType { get; set; }
    public string? Language { get; set; }
    public int? AllowedRevisions { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public CompanyType? CompanyType { get; set; }
    public decimal? CapitalAmount { get; set; }
    public int? FoundersCount { get; set; }
    public bool? HasPowerOfAttorney { get; set; }
    public string? CourtName { get; set; }
    public string? CaseNumber { get; set; }
    public DateTime? NextHearingDate { get; set; }
    public LawsuitStatus? LawsuitStatus { get; set; }
    public ClientRole? ClientRole { get; set; }

    // Collections
    public List<CaseNoteDto> Notes { get; set; } = new();
    public List<CaseTimelineDto> Timelines { get; set; } = new();
    public List<CaseDocumentDto> Documents { get; set; } = new();
}
