using System.Text.Json;
using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Reports.Commands.CreateReport;
using AlMostashar.Application.Features.ClientRequests.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Reports.Queries.GetReportDetails;

public class GetReportDetailsQueryHandler : IRequestHandler<GetReportDetailsQuery, Result<ReportDetailsDto>>
{
    private readonly IAppDbContext _context;

    public GetReportDetailsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ReportDetailsDto>> Handle(
        GetReportDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var report = await _context.Reports
            .Include(r => r.Reporter)
            .Include(r => r.ReportedUser)
            .Include(r => r.Attachments)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.ReportId, cancellationToken);

        if (report == null)
            return Result<ReportDetailsDto>.Failure(new Error("Report.NotFound", Messages.Generic.NotFound("Report")));

        // Deserialize JSON evidence
        var messages = string.IsNullOrEmpty(report.AttachedMessages)
            ? new List<ReportMessageEvidence>()
            : JsonSerializer.Deserialize<List<ReportMessageEvidence>>(report.AttachedMessages) ?? new();

        var dto = new ReportDetailsDto
        {
            Id = report.Id,
            Reason = report.Reason,
            Description = report.Description,
            Status = report.Status,
            CreatedAt = report.CreatedAt,
            ResolvedAt = report.ResolvedAt,
            AdminNotes = report.AdminNotes,
            
            ReporterId = report.ReporterId,
            ReporterName = report.Reporter.FullName,
            ReporterRole = report.Reporter is Lawyer ? UserRole.Lawyer : (report.Reporter is Client ? UserRole.Client : UserRole.Admin),
            
            ReportedUserId = report.ReportedUserId,
            ReportedUserName = report.ReportedUser?.FullName,
            ReportedUserRole = report.ReportedUser == null ? null : 
                               (report.ReportedUser is Lawyer ? UserRole.Lawyer : (report.ReportedUser is Client ? UserRole.Client : UserRole.Admin)),
            
            CaseId = report.CaseId,
            Messages = messages,
            Documents = report.Attachments.Select(document => new DocumentDto
            {
                Id = document.Id,
                DocumentName = document.DocumentName,
                CreatedAt = document.CreatedAt
            }).ToList()
        };

        return Result<ReportDetailsDto>.Success(dto);
    }
}
