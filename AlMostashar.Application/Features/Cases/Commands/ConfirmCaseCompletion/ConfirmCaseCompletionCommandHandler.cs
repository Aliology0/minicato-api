using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Escrows.Services;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Application.Features.Cases.Commands.ConfirmCaseCompletion;

public class ConfirmCaseCompletionCommandHandler : IRequestHandler<ConfirmCaseCompletionCommand, Result<string>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;
    private readonly IEscrowSettlementService _settlementService;
    private readonly ILogger<ConfirmCaseCompletionCommandHandler> _logger;

    public ConfirmCaseCompletionCommandHandler(
        IAppDbContext db,
        ICurrentUserService currentUser,
        INotificationService notificationService,
        IEscrowSettlementService settlementService,
        ILogger<ConfirmCaseCompletionCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _notificationService = notificationService;
        _settlementService = settlementService;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(ConfirmCaseCompletionCommand request, CancellationToken cancellationToken)
    {
        Result<string>? result = null;

        // Variables needed for sending the notification after successful commit
        int lawyerId = 0;
        string caseTitle = string.Empty;
        string clientFullName = string.Empty;
        string? clientAvatarUrl = null;

        try
        {
            await _db.ExecuteInTransactionAsync(async ct =>
            {
                var caseEntity = await _db.Cases
                    .Include(c => c.CaseClientRequest)
                        .ThenInclude(ccr => ccr!.ClientRequest)
                    .FirstOrDefaultAsync(c => c.Id == request.CaseId, ct);

                if (caseEntity is null)
                {
                    result = Result<string>.Failure(new Error("Case.NotFound", Messages.Cases.NotFound));
                    return;
                }

                var clientRequest = caseEntity.CaseClientRequest?.ClientRequest;
                if (clientRequest is null || clientRequest.ClientId != _currentUser.UserId)
                {
                    result = Result<string>.Failure(new Error("Auth.Forbidden", Messages.Auth.CannotViewCase));
                    return;
                }

                if (caseEntity.Status != CaseStatus.PendingConfirmation)
                {
                    result = Result<string>.Failure(new Error(
                        "Case.InvalidStatus",
                        $"Only cases pending confirmation can be confirmed. Current status is {caseEntity.Status}."));
                    return;
                }

                // Find the escrow record associated with the client request
                var escrow = await _db.Escrows
                    .FirstOrDefaultAsync(e => e.RequestId == clientRequest.Id, ct);

                if (escrow is null)
                {
                    result = Result<string>.Failure(new Error("Escrow.NotFound", "Escrow was not found."));
                    return;
                }

                var settlement = await _settlementService.ReleaseToLawyerAsync(
                    escrow.Id,
                    allowDisputedEscrow: false,
                    ct);

                if (!settlement.IsSuccess)
                {
                    result = Result<string>.Failure(settlement.Error!);
                    return;
                }

                // Update case status
                caseEntity.Status = CaseStatus.Closed;
                caseEntity.CancellationReason = null;
                clientRequest.Status = ClientRequestStatus.Completed;

                await _db.SaveChangesAsync(ct);

                // Populate variables for notification
                lawyerId = caseEntity.LawyerId;
                caseTitle = caseEntity.Title;

                var clientUser = await _db.Users
                    .Where(u => u.Id == _currentUser.UserId)
                    .Select(u => new { u.FullName, u.AvatarUrl })
                    .FirstOrDefaultAsync(ct);

                clientFullName = clientUser?.FullName ?? "";
                clientAvatarUrl = clientUser?.AvatarUrl;

                result = Result<string>.Success("Case completion confirmed and escrow released successfully.");
            }, cancellationToken);
        }
        catch (EscrowReleaseConflictException ex)
        {
            result = Result<string>.Failure(new Error("Escrow.ReleaseConflict", ex.Message));
        }

        if (result is null || !result.IsSuccess)
        {
            return result ?? Result<string>.Failure(new Error("Case.ConfirmationFailed", "Failed to confirm case completion."));
        }

        // Send Notification to Lawyer
        try
        {
            var title = Messages.Notifications.CaseCompletedTitle;
            var body = Messages.Notifications.CaseCompletedBody(clientFullName, caseTitle);

            // Persist the notification in the database
            var notificationEntity = new Notification
            {
                UserId = lawyerId,
                Title = title,
                Description = body,
                Type = NotificationType.CaseUpdate,
                SourceId = request.CaseId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _db.Notifications.AddAsync(notificationEntity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // Send real-time notification via NotificationService
            await _notificationService.SendToUserAsync(
                lawyerId,
                title,
                body,
                NotificationType.CaseUpdate,
                request.CaseId,
                clientFullName,
                clientAvatarUrl,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to send CaseCompleted notification to lawyer {LawyerId} for case {CaseId}",
                lawyerId, request.CaseId);
        }

        return result;
    }
}
