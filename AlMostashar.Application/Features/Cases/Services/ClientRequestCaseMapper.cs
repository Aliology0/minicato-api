using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.ClientRequests.RequestDetails;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;

namespace AlMostashar.Application.Features.Cases.Services;

public sealed class ClientRequestCaseMapper : IClientRequestCaseMapper
{
    private readonly ICaseMapper _caseMapper;
    public ClientRequestCaseMapper(ICaseMapper caseMapper) => _caseMapper = caseMapper;

    public Result<Case> Create(ClientRequest request, int lawyerId, object requestDetails)
    {
        var data = requestDetails switch
        {
            ConsultationRequestDetailsDto d => new CaseCreationData(
                request.ServiceType, lawyerId, request.Title, request.ProblemDetails, request.Client.FullName,
                d.PreferredAppointmentDate, d.CommunicationMethod, d.LegalBranch, d.ConsultationSummary),
            ContractRequestDetailsDto d => new CaseCreationData(
                request.ServiceType, lawyerId, request.Title, request.ProblemDetails, request.Client.FullName,
                ContractType: d.ContractType, Language: d.Language, AllowedRevisions: d.AllowedRevisions,
                DeliveryDate: d.DeliveryDate),
            CompanyFormationRequestDetailsDto d => new CaseCreationData(
                request.ServiceType, lawyerId, request.Title, request.ProblemDetails, request.Client.FullName,
                CompanyType: d.CompanyType, CapitalAmount: d.CapitalAmount, FoundersCount: d.FoundersCount,
                HasPowerOfAttorney: d.HasPowerOfAttorney),
            LawsuitRequestDetailsDto d => new CaseCreationData(
                request.ServiceType, lawyerId, request.Title, request.ProblemDetails, request.Client.FullName,
                LegalBranch: d.LegalBranch, CourtName: d.CourtName, CaseNumber: d.CaseNumber,
                NextHearingDate: d.NextHearingDate, LawsuitStatus: d.LawsuitStatus, ClientRole: d.ClientRole),
            GenericRequestDetailsDto => new CaseCreationData(
                request.ServiceType, lawyerId, request.Title, request.ProblemDetails, request.Client.FullName),
            _ => null
        };

        return data is null
            ? Result<Case>.Failure(new Error("Case.UnsupportedRequestDetails", "Unsupported request details type."))
            : Result<Case>.Success(_caseMapper.Create(data));
    }
}
