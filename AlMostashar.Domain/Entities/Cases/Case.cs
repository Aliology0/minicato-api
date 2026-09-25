using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Domain.Entities;

public class Case : BaseEntity
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Title { get; set; }
    public string Description { get; set; }
    public ServiceType ServiceType { get; set; }
    public DateTime CreatedAt { get; set; }
    public CaseStatus Status { get; set; }
    public string? ClientName { get; set; }
    public string? CancellationReason { get; set; }

    // Navigation — 1:N (Case → Timelines, Notes, Documents)
    public ICollection<CaseTimeline> Timelines { get; set; } = new List<CaseTimeline>();
    public ICollection<CaseNotes> Notes { get; set; } = new List<CaseNotes>();
    public ICollection<CaseDocuments> Documents { get; set; } = new List<CaseDocuments>();

    // 1:1 Partial — Case ↔ ClientRequest (via CaseClientRequest)
    public CaseClientRequest? CaseClientRequest { get; set; }

    // 1:1 Partial — Case ↔ Chat (FK on Chat side, total participation)
    public Chat? Chat { get; set; }

    // FK — N:1 Mandatory (Case must belong to a Lawyer)
    public int LawyerId { get; set; }
    public Lawyer Lawyer { get; set; }

    // Navigation — 1:N (Case → Reports)
    public ICollection<Report> Reports { get; set; } = new List<Report>();
}

public class IntellectualPropertyCase : Case
{
}

public class LegalTranslationCase : Case
{
}

public class DueDiligenceCase : Case
{
}

public class DebtCollectionCase : Case
{
}

public class MediationCase : Case
{
}

public class LegalReviewCase : Case
{
}

public class ComplianceCase : Case
{
}

// 1. Consultation Case
public class ConsultationCase : Case
{
    public DateTime? AppointmentDate { get; set; }
    public CommunicationMethod CommunicationMethod { get; set; }
    public string LegalBranch { get; set; } = string.Empty;
    public string? ConsultationSummary { get; set; }
}

// 2. Contract Case
public class ContractCase : Case
{
    public ContractType ContractType { get; set; }
    public string Language { get; set; } = string.Empty;
    public int AllowedRevisions { get; set; }
    public int UsedRevisions { get; set; }
    public DateTime? DeliveryDate { get; set; }
}

// 3. Company Formation Case
public class CompanyFormationCase : Case
{
    public CompanyType CompanyType { get; set; }
    public decimal CapitalAmount { get; set; }
    public int FoundersCount { get; set; }
    public bool HasPowerOfAttorney { get; set; }
    public string? CommercialRegistrationNo { get; set; }
}

// 4. Lawsuit Case
public class LawsuitCase : Case
{
    public string LegalBranch { get; set; } = string.Empty;
    public string CourtName { get; set; } = string.Empty;
    public string CaseNumber { get; set; } = string.Empty;
    public DateTime? NextHearingDate { get; set; }
    public LawsuitStatus LawsuitStatus { get; set; }
    public ClientRole ClientRole { get; set; }
}
