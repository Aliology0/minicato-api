using System.Text.Json;
using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Reports.Commands.CreateReport;

public class CreateReportCommandHandler : IRequestHandler<CreateReportCommand, Result<string>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateReportCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<string>> Handle(CreateReportCommand request, CancellationToken cancellationToken)
    {
        var reporterId = _currentUser.UserId;

        // ── Resolve ReportedUserId ─────────────────────────────────────────────
        int? reportedUserId = null;

        if (request.LawyerId.HasValue)
        {
            // Lawyer is a User (TPH) — Lawyer.Id IS the User.Id
            var lawyerExists = await _context.Lawyers
                .AnyAsync(l => l.Id == request.LawyerId.Value, cancellationToken);

            if (!lawyerExists)
                return Result<string>.Failure(new Error("Lawyer.NotFound", Messages.Generic.NotFound("Lawyer")));

            reportedUserId = request.LawyerId.Value;
        }
        else if (request.UserId.HasValue)
        {
            var userExists = await _context.Users
                .AnyAsync(u => u.Id == request.UserId.Value, cancellationToken);

            if (!userExists)
                return Result<string>.Failure(new Error("User.NotFound", Messages.Generic.NotFound("User")));

            reportedUserId = request.UserId.Value;
        }
        // else: platform-level report — reportedUserId stays null

        // ── Prevent self-reporting ─────────────────────────────────────────────
        if (reportedUserId.HasValue && reportedUserId.Value == reporterId)
            return Result<string>.Failure(new Error("Report.SelfReport", "You cannot file a report against yourself."));

        // ── Validate CaseId ────────────────────────────────────────────────────
        if (request.CaseId.HasValue)
        {
            var caseEntity = await _context.Cases
                .Include(c => c.CaseClientRequest).ThenInclude(ccr => ccr.ClientRequest)
                .FirstOrDefaultAsync(c => c.Id == request.CaseId.Value, cancellationToken);

            if (caseEntity == null)
                return Result<string>.Failure(new Error("Case.NotFound", Messages.Generic.NotFound("Case")));

            // Validate that the reporter is part of this case
            bool isReporterInvolved = caseEntity.LawyerId == reporterId || 
                                      (caseEntity.CaseClientRequest?.ClientRequest?.ClientId == reporterId);

            if (!isReporterInvolved)
                return Result<string>.Failure(new Error("Case.Unauthorized", "You are not a participant in this case."));

            // If a reported user is specified, validate they are also part of this case
            if (reportedUserId.HasValue)
            {
                bool isReportedInvolved = caseEntity.LawyerId == reportedUserId.Value || 
                                          (caseEntity.CaseClientRequest?.ClientRequest?.ClientId == reportedUserId.Value);
                if (!isReportedInvolved)
                    return Result<string>.Failure(new Error("Case.InvalidReportedUser", "The reported user is not a participant in this case."));
            }
        }

        // ── Serialize attached messages ────────────────────────────────────────
        // Since chats are end-to-end encrypted and can only be decrypted on the user's
        // device, we accept the already-decrypted messages from the frontend's local DB
        // and store them as-is in JSON format.
        string? attachedMessagesJson = null;
        if (request.Messages is { Count: > 0 })
        {
            attachedMessagesJson = JsonSerializer.Serialize(request.Messages);
        }

        // ── Serialize attachment URLs ──────────────────────────────────────────
        var attachmentIds = request.AttachmentIds?.Distinct().ToArray() ?? Array.Empty<int>();
        var attachments = attachmentIds.Length == 0
            ? new List<CaseDocuments>()
            : await _context.CaseDocuments
                .Where(document => attachmentIds.Contains(document.Id)
                    && document.UploadedByUserId == reporterId
                    && document.ClientRequestId == null
                    && document.CaseId == null
                    && document.ReportId == null
                    && document.CleanupClaimToken == null)
                .ToListAsync(cancellationToken);

        if (attachments.Count != attachmentIds.Length)
            return Result<string>.Failure(new Error(
                "Report.InvalidAttachments",
                "One or more documents are missing, owned by another user, or already linked."));

        // ── Persist ───────────────────────────────────────────────────────────
        var report = new Report
        {
            Reason = request.Reason,
            Status = ReportStatus.Pending,
            Description = request.ProblemContent,
            AttachedMessages = attachedMessagesJson,
            ReporterId = reporterId,
            ReportedUserId = reportedUserId,
            CaseId = request.CaseId,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var attachment in attachments)
            attachment.Report = report;

        _context.Reports.Add(report);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(Messages.GeneralSuccess.ReportSubmitted);
    }
}
