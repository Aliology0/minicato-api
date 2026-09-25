
using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Domain.Entities;

// Step 1: Regular Entity (Supertype for EERD Specialization)
public class ClientRequest: BaseEntity
{
    public string RequestId { get; set; }
    public DateTime CreatedAt { get; set; }
    public ClientRequestStatus Status { get; set; }
    public string Title { get; set; }
    public string ProblemDetails { get; set; }
    public int? GovernorateId { get; set; }
    public string Governorate { get; set; } = string.Empty;
    public int? CityId { get; set; }
    public string? City { get; set; }
    public int? LegalServiceId { get; set; }
    public ServiceType ServiceType { get; set; } = ServiceType.Base;
    public DateTime? ClientDeadline { get; set; }
    public CommunicationMethod? PreferredCommunicationMethod { get; set; }
    public RequestUrgency Urgency { get; set; } = RequestUrgency.Normal;
    public ConsultationRequestDetails? ConsultationDetails { get; set; }
    public ContractRequestDetails? ContractDetails { get; set; }
    public LawsuitRequestDetails? LawsuitDetails { get; set; }
    public CompanyFormationRequestDetails? CompanyFormationDetails { get; set; }
    public GenericRequestDetails? GenericDetails { get; set; }
    public int? AcceptedOfferId { get; set; }

    // Navigation — 1:N (ClientRequest → Documents)
    public ICollection<CaseDocuments> Documents { get; set; } = new List<CaseDocuments>();
    public ICollection<RequestOffer> Offers { get; set; } = new List<RequestOffer>();

    // FK — Step 4: 1:N (Client → ClientRequest)
    public int ClientId { get; set; }
    public Client Client { get; set; }

    // FK — N:1 (ClientRequest → LawyerService) — composite FK matching LawyerService's composite PK
    public int? LawyerServiceLawyerId { get; set; }
    public int? LawyerServiceLegalServiceId { get; set; }
    public LawyerService? LawyersServices { get; set; }
    public LegalService? RequestedLegalService { get; set; }
    // 1:1 Partial — ClientRequest ↔ Case (via CaseClientRequest)
    public CaseClientRequest? CaseClientRequest { get; set; }

    // Navigation — Step 3: 1:1 (ClientRequest → Invoice)
    public Invoice Invoice { get; set; }

    // Navigation — Step 3: 1:1 (ClientRequest → Escrow)
    public Escrow Escrow { get; set; }

    // Navigation — Step 3: 1:1 (ClientRequest → Feedback)
    public Feedback Feedback { get; set; }

    public void MarkAccepted(
        int? lawyerId,
        int? legalServiceId,
        decimal totalAmount,
        decimal platformFee,
        decimal lawyerAmount)
    {
        Status = ClientRequestStatus.Accepted;
        LegalServiceId = legalServiceId ?? LegalServiceId;
        LawyerServiceLawyerId = lawyerId;
        LawyerServiceLegalServiceId = legalServiceId;
        AddEvent(new Domain.Events.RequestAcceptedEvent(this, totalAmount, platformFee, lawyerAmount));
    }

    public void MarkAcceptedOffer(int offerId) => AcceptedOfferId = offerId;
}
