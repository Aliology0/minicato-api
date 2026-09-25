using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Application.Features.Cases.DTOs;

public class CaseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ServiceType ServiceType { get; set; }
    public CaseStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Platform if linked via CaseClientRequest, External if manual.</summary>
    public CaseSource Source { get; set; }

    public string? ClientName { get; set; }
    public string? Reference { get; set; }
    public int LawyerId { get; set; }
    public int? ClientRequestId { get; set; }
    public string? ClientRequestReference { get; set; }
    public int? ServiceId { get; set; }
    public string? CancellationReason { get; set; }
    public CaseChatDto? Chat { get; set; }
}

public class CaseChatDto
{
    public int ChatId { get; set; }
    public int ReceiverId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? ProfileImage { get; set; }
    public int UnreadMessagesCount { get; set; }
}
