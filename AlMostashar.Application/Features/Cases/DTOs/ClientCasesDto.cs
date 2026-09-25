using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Application.Features.Cases.DTOs;

public class ClientCasesDto
{
    public int CaseId { get; set; }
    public string ClientRequestId { get; set; } = string.Empty;
    public int ClientRequestNumericId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string CaseTitle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CaseNo { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public CaseStatus Status { get; set; }
    public int LawyerId { get; set; }
    public string LawyerName { get; set; } = string.Empty;
    public ServiceType ServiceType { get; set; }
    public int? ServiceId { get; set; }
    public int? ChatId { get; set; }
    public string? CancellationReason { get; set; }
    
    public List<CaseDocumentDto> Docs { get; set; } = new();
    public List<CaseTimelineDto> Timeline { get; set; } = new();
    
}
