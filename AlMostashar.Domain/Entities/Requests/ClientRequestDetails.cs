using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Domain.Entities;

public sealed class ConsultationRequestDetails
{
    public int ClientRequestId { get; set; }
    public ClientRequest ClientRequest { get; set; } = null!;
    public string LegalBranch { get; set; } = string.Empty;
    public DateTime? PreferredAppointmentDate { get; set; }
    public CommunicationMethod CommunicationMethod { get; set; }
    public string? ConsultationSummary { get; set; }
}

public sealed class ContractRequestDetails
{
    public int ClientRequestId { get; set; }
    public ClientRequest ClientRequest { get; set; } = null!;
    public ContractType ContractType { get; set; }
    public ContractRequestType ContractRequestType { get; set; }
    public string Language { get; set; } = string.Empty;
    public int? PagesCount { get; set; }
    public int? AllowedRevisions { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string? OtherPartyName { get; set; }
}

public sealed class LawsuitRequestDetails
{
    public int ClientRequestId { get; set; }
    public ClientRequest ClientRequest { get; set; } = null!;
    public string LegalBranch { get; set; } = string.Empty;
    public bool IsCaseAlreadyFiled { get; set; }
    public string? CourtName { get; set; }
    public string? CaseNumber { get; set; }
    public DateTime? NextHearingDate { get; set; }
    public LawsuitStatus? LawsuitStatus { get; set; }
    public ClientRole? ClientRole { get; set; }
    public string? OpponentName { get; set; }
}

public sealed class CompanyFormationRequestDetails
{
    public int ClientRequestId { get; set; }
    public ClientRequest ClientRequest { get; set; } = null!;
    public CompanyType CompanyType { get; set; }
    public string BusinessActivity { get; set; } = string.Empty;
    public decimal? CapitalAmount { get; set; }
    public int FoundersCount { get; set; }
    public bool HasPowerOfAttorney { get; set; }
    public string? ProposedCompanyName { get; set; }
}

public sealed class GenericRequestDetails
{
    public int ClientRequestId { get; set; }
    public ClientRequest ClientRequest { get; set; } = null!;
    public string? LegalBranch { get; set; }
    public string? Summary { get; set; }
    public string? DesiredOutcome { get; set; }
    public string? ImportantDates { get; set; }
}
