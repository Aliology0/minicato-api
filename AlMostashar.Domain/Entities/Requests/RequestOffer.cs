using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Domain.Entities;

public class RequestOffer : BaseEntity
{
    public int ClientRequestId { get; set; }
    public ClientRequest ClientRequest { get; set; } = null!;

    public int LawyerId { get; set; }
    public Lawyer Lawyer { get; set; } = null!;

    public int LegalServiceId { get; set; }
    public LegalService LegalService { get; set; } = null!;

    public decimal OfferedAmount { get; set; }
    public string? Note { get; set; }

    public OfferStatus Status { get; set; } = OfferStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }
}
