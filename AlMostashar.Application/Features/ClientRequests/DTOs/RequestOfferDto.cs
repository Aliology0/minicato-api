using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Application.Features.ClientRequests.DTOs;

public class RequestOfferDto
{
    public int OfferId { get; set; }
    public int RequestId { get; set; }
    public string RequestTitle { get; set; } = null!;

    public int LawyerId { get; set; }
    public string LawyerName { get; set; } = null!;
    public string? LawyerProfileImage { get; set; }

    public int LegalServiceId { get; set; }
    public string LegalServiceTitle { get; set; } = null!;

    public decimal OfferedAmount { get; set; }
    public string? Note { get; set; }

    public OfferStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}
