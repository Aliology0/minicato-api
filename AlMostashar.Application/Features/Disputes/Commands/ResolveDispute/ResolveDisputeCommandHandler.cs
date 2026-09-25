using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Escrows.Commands.RefundEscrow;
using AlMostashar.Application.Features.Escrows.Commands.ReleaseEscrow;
using AlMostashar.Application.Features.Payments.Commands.RefundPayment;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Application.Features.Disputes.Commands.ResolveDispute;

public class ResolveDisputeCommandHandler : IRequestHandler<ResolveDisputeCommand, Result<string>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IMediator _mediator;
    private readonly ILogger<ResolveDisputeCommandHandler> _logger;

    public ResolveDisputeCommandHandler(
        IAppDbContext context,
        ICurrentUserService currentUser,
        IMediator mediator,
        ILogger<ResolveDisputeCommandHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(ResolveDisputeCommand request, CancellationToken cancellationToken)
    {
        var dispute = await _context.Disputes
            .FirstOrDefaultAsync(d => d.Id == request.DisputeId, cancellationToken);

        if (dispute == null)
            return Result<string>.Failure(new Error("Dispute.NotFound", Messages.Generic.NotFound("Dispute")));

        if (dispute.Status == DisputeStatus.ResolvedRelease || 
            dispute.Status == DisputeStatus.ResolvedRefund || 
            dispute.Status == DisputeStatus.Dismissed)
        {
            return Result<string>.Failure(new Error("Dispute.AlreadyResolved", "This dispute has already been resolved or dismissed."));
        }

        // Apply resolution
        dispute.AdminDecision = request.AdminDecision;
        dispute.AdminNotes = request.AdminNotes;
        dispute.ReviewedByAdminId = _currentUser.UserId;
        dispute.ResolvedAt = DateTime.UtcNow;

        switch (request.ResolutionType)
        {
            case DisputeResolutionType.ReleaseToLawyer:
                dispute.Status = DisputeStatus.ResolvedRelease;
                if (dispute.EscrowId.HasValue)
                {
                    var releaseResult = await _mediator.Send(new ReleaseEscrowCommand(dispute.EscrowId.Value, AllowDisputedEscrow: true), cancellationToken);
                    if (!releaseResult.IsSuccess) return Result<string>.Failure(releaseResult.Error!);
                    await _context.Cases
                    .Where(c => c.Id == dispute.CaseId)
                    .ExecuteUpdateAsync(setter => setter.SetProperty(c => c.Status, CaseStatus.Dismissed));

                    // Keep ClientRequest status in sync with Case
                    await _context.ClientRequests
                        .Where(cr => _context.CaseClientRequests
                            .Where(ccr => ccr.CaseId == dispute.CaseId)
                            .Select(ccr => ccr.ClientRequestId)
                            .Contains(cr.Id))
                        .ExecuteUpdateAsync(setter => setter.SetProperty(cr => cr.Status, ClientRequestStatus.Cancelled), cancellationToken);
                }
                break;

            case DisputeResolutionType.RefundToClient:
                dispute.Status = DisputeStatus.ResolvedRefund;

                if (dispute.EscrowId.HasValue)
                {
                    var paymentId = await _context.Escrows
                        .Where(e => e.Id == dispute.EscrowId.Value)
                        .Select(e => e.PaymentId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (!paymentId.HasValue)
                        return Result<string>.Failure(
                            new Error("Payment.NotFound", "Escrow does not have a linked payment."));

                    // Refund the payment via Paymob first
                    var refundPaymentResult = await _mediator.Send(
                        new RefundPaymentCommand(paymentId.Value),
                        cancellationToken);

                    if (!refundPaymentResult.IsSuccess)
                        return Result<string>.Failure(refundPaymentResult.Error!);
                    await _context.Cases
                        .Where(c => c.Id == dispute.CaseId)
                        .ExecuteUpdateAsync(setter => setter.SetProperty(c => c.Status, CaseStatus.Dismissed));

                    // Keep ClientRequest status in sync with Case
                    await _context.ClientRequests
                        .Where(cr => _context.CaseClientRequests
                            .Where(ccr => ccr.CaseId == dispute.CaseId)
                            .Select(ccr => ccr.ClientRequestId)
                            .Contains(cr.Id))
                        .ExecuteUpdateAsync(setter => setter.SetProperty(cr => cr.Status, ClientRequestStatus.Cancelled), cancellationToken);
                        
                    // Temporary safeguard until Outbox Pattern is implemented.
                    // If Paymob refund succeeds but escrow update fails, we log the inconsistency
                    // and return failure so admin can retry/manual-review.
                    try
                    {
                        var refundEscrowResult = await _mediator.Send(
                            new RefundEscrowCommand(dispute.EscrowId.Value),
                            cancellationToken);

                        if (!refundEscrowResult.IsSuccess)
                        {
                            _logger.LogError(
                                "Paymob refund succeeded but RefundEscrowCommand failed. " +
                                "DisputeId={DisputeId}, EscrowId={EscrowId}, PaymentId={PaymentId}, " +
                                "ErrorCode={ErrorCode}, ErrorMessage={ErrorMessage}",
                                dispute.Id,
                                dispute.EscrowId.Value,
                                paymentId.Value,
                                refundEscrowResult.Error!.Code,
                                refundEscrowResult.Error.Message);

                            return Result<string>.Failure(refundEscrowResult.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Paymob refund succeeded but RefundEscrowCommand threw exception. " +
                            "DisputeId={DisputeId}, EscrowId={EscrowId}, PaymentId={PaymentId}",
                            dispute.Id,
                            dispute.EscrowId.Value,
                            paymentId.Value);

                        return Result<string>.Failure(
                            new Error(
                                "Escrow.RefundStateUpdateFailed",
                                "Payment refund succeeded, but escrow state update failed. Please retry or handle manually."));
                    }
                }

                break;

            case DisputeResolutionType.Dismiss:
                dispute.Status = DisputeStatus.Dismissed;
                break;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("Dispute resolved successfully.");
    }
}
