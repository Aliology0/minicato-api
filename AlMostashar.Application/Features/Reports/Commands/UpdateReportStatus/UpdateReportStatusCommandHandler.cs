using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Reports.Commands.UpdateReportStatus;

public class UpdateReportStatusCommandHandler : IRequestHandler<UpdateReportStatusCommand, Result<string>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateReportStatusCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<string>> Handle(UpdateReportStatusCommand request, CancellationToken cancellationToken)
    {
        var report = await _context.Reports
            .FirstOrDefaultAsync(r => r.Id == request.ReportId, cancellationToken);

        if (report == null)
            return Result<string>.Failure(new Error("Report.NotFound", Messages.Generic.NotFound("Report")));

        report.Status = request.Status;
        report.AdminNotes = request.AdminNotes;
        report.ReviewedByAdminId = _currentUser.UserId;

        // Set ResolvedAt if moving to a terminal status
        if (request.Status == ReportStatus.Resolved || request.Status == ReportStatus.Dismissed)
        {
            report.ResolvedAt = DateTime.UtcNow;
        }
        else
        {
            report.ResolvedAt = null; // Clear if moved back to Pending/UnderReview
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("Report status updated successfully.");
    }
}
