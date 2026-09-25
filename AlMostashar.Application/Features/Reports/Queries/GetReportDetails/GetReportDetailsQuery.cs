using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Reports.Queries.GetReportDetails;

public class GetReportDetailsQuery : IRequest<Result<ReportDetailsDto>>
{
    public int ReportId { get; set; }

    public GetReportDetailsQuery(int reportId)
    {
        ReportId = reportId;
    }
}
