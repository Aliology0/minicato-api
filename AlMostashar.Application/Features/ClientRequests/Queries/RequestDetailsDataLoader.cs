using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.ClientRequests.RequestDetails;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.ClientRequests.Queries;

internal static class RequestDetailsDataLoader
{
    public static async Task<Dictionary<int, object>> LoadAsync(IAppDbContext db, IReadOnlyCollection<int> ids, CancellationToken ct)
    {
        var result = new Dictionary<int, object>();
        if (ids.Count == 0) return result;
        foreach (var row in await db.ConsultationRequestDetails.AsNoTracking().Where(x => ids.Contains(x.ClientRequestId)).ToListAsync(ct))
            result[row.ClientRequestId] = new ConsultationRequestDetailsDto(row.LegalBranch, row.PreferredAppointmentDate, row.CommunicationMethod, row.ConsultationSummary);
        foreach (var row in await db.ContractRequestDetails.AsNoTracking().Where(x => ids.Contains(x.ClientRequestId)).ToListAsync(ct))
            result[row.ClientRequestId] = new ContractRequestDetailsDto(row.ContractType, row.ContractRequestType, row.Language, row.PagesCount, row.AllowedRevisions, row.DeliveryDate, row.OtherPartyName);
        foreach (var row in await db.LawsuitRequestDetails.AsNoTracking().Where(x => ids.Contains(x.ClientRequestId)).ToListAsync(ct))
            result[row.ClientRequestId] = new LawsuitRequestDetailsDto(row.LegalBranch, row.IsCaseAlreadyFiled, row.CourtName, row.CaseNumber, row.NextHearingDate, row.LawsuitStatus, row.ClientRole, row.OpponentName);
        foreach (var row in await db.CompanyFormationRequestDetails.AsNoTracking().Where(x => ids.Contains(x.ClientRequestId)).ToListAsync(ct))
            result[row.ClientRequestId] = new CompanyFormationRequestDetailsDto(row.CompanyType, row.BusinessActivity, row.CapitalAmount, row.FoundersCount, row.HasPowerOfAttorney, row.ProposedCompanyName);
        foreach (var row in await db.GenericRequestDetails.AsNoTracking().Where(x => ids.Contains(x.ClientRequestId)).ToListAsync(ct))
            result[row.ClientRequestId] = new GenericRequestDetailsDto(row.LegalBranch, row.Summary, row.DesiredOutcome, row.ImportantDates);
        return result;
    }
}
