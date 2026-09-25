using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using System.Text.Json.Serialization;

namespace AlMostashar.Application.Features.Reports.Commands.UpdateReportStatus;

public class UpdateReportStatusCommand : IRequest<Result<string>>
{
    [JsonIgnore]
    public int ReportId { get; set; }

    public ReportStatus Status { get; set; }
    
    public string? AdminNotes { get; set; }
}
