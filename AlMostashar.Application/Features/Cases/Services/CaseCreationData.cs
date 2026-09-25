using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Application.Features.Cases.Services;

public sealed record CaseCreationData(
    ServiceType ServiceType,
    int LawyerId,
    string Title,
    string Description,
    string? ClientName = null,
    DateTime? AppointmentDate = null,
    CommunicationMethod? CommunicationMethod = null,
    string? LegalBranch = null,
    string? ConsultationSummary = null,
    ContractType? ContractType = null,
    string? Language = null,
    int? AllowedRevisions = null,
    DateTime? DeliveryDate = null,
    CompanyType? CompanyType = null,
    decimal? CapitalAmount = null,
    int? FoundersCount = null,
    bool? HasPowerOfAttorney = null,
    string? CourtName = null,
    string? CaseNumber = null,
    DateTime? NextHearingDate = null,
    LawsuitStatus? LawsuitStatus = null,
    ClientRole? ClientRole = null);
