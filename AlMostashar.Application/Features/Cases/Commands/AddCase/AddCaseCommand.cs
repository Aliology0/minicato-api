using AlMostashar.Application.Features.Cases.DTOs;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;

namespace AlMostashar.Application.Features.Cases.Commands.AddCase;

public record AddCaseCommand(
    string Title,
    string Description,
    string? ClientName,
    ServiceType ServiceType,
    // Consultation
    DateTime? AppointmentDate,
    CommunicationMethod? CommunicationMethod,
    string? LegalBranch,
    // Contract
    ContractType? ContractType,
    string? Language,
    int? AllowedRevisions,
    DateTime? DeliveryDate,
    // Company Formation
    CompanyType? CompanyType,
    decimal? CapitalAmount,
    int? FoundersCount,
    bool? HasPowerOfAttorney,
    // Lawsuit
    string? CourtName,
    string? CaseNumber,
    DateTime? NextHearingDate,
    LawsuitStatus? LawsuitStatus,
    ClientRole? ClientRole,
    // Generic creation options
    int? LawyerId = null,
    int? ClientRequestId = null,
    bool PublishCaseCreatedEvent = false
) : IRequest<Result<CaseDetailDto>>;
