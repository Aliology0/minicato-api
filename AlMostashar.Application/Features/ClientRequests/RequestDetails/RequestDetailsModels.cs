using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Application.Features.ClientRequests.RequestDetails;

public sealed record ConsultationRequestDetailsDto(
    string LegalBranch,
    DateTime? PreferredAppointmentDate,
    CommunicationMethod CommunicationMethod,
    string? ConsultationSummary);

public sealed record ContractRequestDetailsDto(
    ContractType ContractType,
    ContractRequestType ContractRequestType,
    string Language,
    int? PagesCount,
    int? AllowedRevisions,
    DateTime? DeliveryDate,
    string? OtherPartyName);

public sealed record LawsuitRequestDetailsDto(
    string LegalBranch,
    bool IsCaseAlreadyFiled,
    string? CourtName,
    string? CaseNumber,
    DateTime? NextHearingDate,
    LawsuitStatus? LawsuitStatus,
    ClientRole? ClientRole,
    string? OpponentName);

public sealed record CompanyFormationRequestDetailsDto(
    CompanyType CompanyType,
    string BusinessActivity,
    decimal? CapitalAmount,
    int FoundersCount,
    bool HasPowerOfAttorney,
    string? ProposedCompanyName);

public sealed record GenericRequestDetailsDto(
    string? LegalBranch,
    string? Summary,
    string? DesiredOutcome,
    string? ImportantDates);

public sealed record ValidatedRequestDetails(object Details);
