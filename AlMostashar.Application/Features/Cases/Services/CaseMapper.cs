using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Entities;

namespace AlMostashar.Application.Features.Cases.Services;

public sealed class CaseMapper : ICaseMapper
{
    public Case Create(CaseCreationData data)
    {
        var entity = CaseFactory.Create(data.ServiceType, data.LawyerId, data.Title, data.Description);
        entity.ClientName = data.ClientName;

        switch (entity)
        {
            case ConsultationCase value:
                value.AppointmentDate = data.AppointmentDate;
                value.CommunicationMethod = data.CommunicationMethod ?? default;
                value.LegalBranch = data.LegalBranch ?? string.Empty;
                value.ConsultationSummary = data.ConsultationSummary;
                break;
            case ContractCase value:
                value.ContractType = data.ContractType ?? default;
                value.Language = data.Language ?? string.Empty;
                value.AllowedRevisions = data.AllowedRevisions ?? 0;
                value.DeliveryDate = data.DeliveryDate;
                break;
            case CompanyFormationCase value:
                value.CompanyType = data.CompanyType ?? default;
                value.CapitalAmount = data.CapitalAmount ?? 0;
                value.FoundersCount = data.FoundersCount ?? 0;
                value.HasPowerOfAttorney = data.HasPowerOfAttorney ?? false;
                break;
            case LawsuitCase value:
                value.LegalBranch = data.LegalBranch ?? string.Empty;
                value.CourtName = data.CourtName ?? string.Empty;
                value.CaseNumber = data.CaseNumber ?? string.Empty;
                value.NextHearingDate = data.NextHearingDate;
                value.LawsuitStatus = data.LawsuitStatus ?? default;
                value.ClientRole = data.ClientRole ?? default;
                break;
        }

        return entity;
    }
}
