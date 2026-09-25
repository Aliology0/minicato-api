using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Application.Features.Cases.Commands.FinishCase;

public class FinishCaseCommandHandler : IRequestHandler<FinishCaseCommand, Result<string>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;
    private readonly ILogger<FinishCaseCommandHandler> _logger;

    public FinishCaseCommandHandler(
        IAppDbContext db,
        ICurrentUserService currentUser,
        INotificationService notificationService,
        ILogger<FinishCaseCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(FinishCaseCommand request, CancellationToken cancellationToken)
    {
        var caseEntity = await _db.Cases
            .Include(c => c.CaseClientRequest)
                .ThenInclude(ccr => ccr!.ClientRequest)
            .FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);

        if (caseEntity is null)
            return Result<string>.Failure(new Error("Case.NotFound", Messages.Cases.NotFound));

        if (caseEntity.LawyerId != _currentUser.UserId)
            return Result<string>.Failure(new Error("Auth.Forbidden", Messages.Auth.NotCaseOwner));

        if (!IsTransitionAllowed(caseEntity.Status))
            return Result<string>.Failure(new Error(
                "Case.InvalidTransition",
                $"Cannot transition from {caseEntity.Status} to {CaseStatus.PendingConfirmation}."));

        caseEntity.Status = CaseStatus.PendingConfirmation;
        caseEntity.CancellationReason = null;

        if (caseEntity.CaseClientRequest?.ClientRequest is not null)
        {
            caseEntity.CaseClientRequest.ClientRequest.Status = ClientRequestStatus.InProgress;
        }

        await _db.SaveChangesAsync(cancellationToken);

        // Send Notification to Client
        if (caseEntity.CaseClientRequest?.ClientRequest is not null)
        {
            var clientId = caseEntity.CaseClientRequest.ClientRequest.ClientId;
            try
            {
                var lawyerUser = await _db.Users
                    .Where(u => u.Id == _currentUser.UserId)
                    .Select(u => new { u.FullName, u.AvatarUrl })
                    .FirstOrDefaultAsync(cancellationToken);

                var statusName = Messages.Enums.GetCaseStatus(CaseStatus.PendingConfirmation);
                var title = Messages.Notifications.CaseStatusUpdatedTitle;
                var body = Messages.Notifications.CaseStatusUpdatedBody(caseEntity.Title, statusName);

                // Persist the notification in the database
                var notificationEntity = new Notification
                {
                    UserId = clientId,
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
                    clientId,
                    title,
                    body,
                    NotificationType.CaseUpdate,
                    request.CaseId,
                    lawyerUser?.FullName,
                    lawyerUser?.AvatarUrl,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to send CaseStatusUpdated notification to client {ClientId} for case {CaseId}",
                    clientId, request.CaseId);
            }
        }

        return Result<string>.Success($"Status updated to {CaseStatus.PendingConfirmation}.");
    }

    private static bool IsTransitionAllowed(CaseStatus currentStatus)
    {
        return currentStatus switch
        {
            CaseStatus.InProgress => true,
            _ => false
        };
    }
}
