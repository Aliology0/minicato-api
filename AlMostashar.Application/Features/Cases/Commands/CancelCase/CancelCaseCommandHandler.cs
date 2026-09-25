using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Escrows.Commands.RefundEscrow;
using AlMostashar.Application.Features.Payments.Commands.RefundPayment;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Application.Features.Cases.Commands.CancelCase;

public class CancelCaseCommandHandler : IRequestHandler<CancelCaseCommand, Result<string>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;
    private readonly IMediator _mediator;
    private readonly ILogger<CancelCaseCommandHandler> _logger;

    public CancelCaseCommandHandler(
        IAppDbContext db,
        ICurrentUserService currentUser,
        INotificationService notificationService,
        IMediator mediator,
        ILogger<CancelCaseCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _notificationService = notificationService;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(CancelCaseCommand request, CancellationToken cancellationToken)
    {
        var caseEntity = await _db.Cases
            .Include(c => c.CaseClientRequest)
                .ThenInclude(ccr => ccr!.ClientRequest)
                    .ThenInclude(cr => cr.Escrow)
            .FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);

        if (caseEntity is null)
            return Result<string>.Failure(new Error("Case.NotFound", Messages.Cases.NotFound));

        if (caseEntity.LawyerId != _currentUser.UserId)
            return Result<string>.Failure(new Error("Auth.Forbidden", Messages.Auth.NotCaseOwner));

        if (!IsTransitionAllowed(caseEntity.Status))
            return Result<string>.Failure(new Error(
                "Case.InvalidTransition",
                $"Cannot transition from {caseEntity.Status} to {CaseStatus.Canceled}."));

        if (string.IsNullOrWhiteSpace(request.Reason))
            return Result<string>.Failure(new Error(
                "Case.CancellationReasonRequired",
                "Cancellation reason is required when canceling a case."));

        // Refund logic if funded/disputed escrow exists
        var escrow = caseEntity.CaseClientRequest?.ClientRequest?.Escrow;
        if (escrow is not null && escrow.Status is EscrowStatus.Funded or EscrowStatus.Disputed)
        {
            if (!escrow.PaymentId.HasValue)
            {
                return Result<string>.Failure(new Error("Payment.NotFound", "Escrow does not have a linked payment."));
            }

            var refundPaymentResult = await _mediator.Send(
                new RefundPaymentCommand(escrow.PaymentId.Value),
                cancellationToken);

            if (!refundPaymentResult.IsSuccess)
                return Result<string>.Failure(refundPaymentResult.Error!);

            try
            {
                var refundEscrowResult = await _mediator.Send(
                    new RefundEscrowCommand(escrow.Id),
                    cancellationToken);

                if (!refundEscrowResult.IsSuccess)
                {
                    _logger.LogError(
                        "Paymob refund succeeded but RefundEscrowCommand failed when lawyer canceled case. " +
                        "CaseId={CaseId}, EscrowId={EscrowId}, PaymentId={PaymentId}, " +
                        "ErrorCode={ErrorCode}, ErrorMessage={ErrorMessage}",
                        caseEntity.Id,
                        escrow.Id,
                        escrow.PaymentId.Value,
                        refundEscrowResult.Error!.Code,
                        refundEscrowResult.Error.Message);

                    return Result<string>.Failure(refundEscrowResult.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Paymob refund succeeded but RefundEscrowCommand threw exception when lawyer canceled case. " +
                    "CaseId={CaseId}, EscrowId={EscrowId}, PaymentId={PaymentId}",
                    caseEntity.Id,
                    escrow.Id,
                    escrow.PaymentId.Value);

                return Result<string>.Failure(
                    new Error(
                        "Escrow.RefundStateUpdateFailed",
                        "Payment refund succeeded, but escrow state update failed. Please retry or handle manually."));
            }
        }

        caseEntity.Status = CaseStatus.Canceled;
        caseEntity.CancellationReason = request.Reason.Trim();

        if (caseEntity.CaseClientRequest?.ClientRequest is not null)
        {
            caseEntity.CaseClientRequest.ClientRequest.Status = ClientRequestStatus.Cancelled;
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

                var statusName = Messages.Enums.GetCaseStatus(CaseStatus.Canceled);
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

        return Result<string>.Success($"Status updated to {CaseStatus.Canceled}.");
    }

    private static bool IsTransitionAllowed(CaseStatus currentStatus)
    {
        return currentStatus switch
        {
            CaseStatus.Open => true,
            CaseStatus.InProgress => true,
            CaseStatus.OnHold => true,
            CaseStatus.PendingConfirmation => true,
            _ => false
        };
    }
}
