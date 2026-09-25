using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Wallets.DTOs;
using AlMostashar.Application.Features.Wallets.Helpers;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Wallets.Commands.MarkWithdrawalPaid;

public class MarkWithdrawalPaidCommandHandler : IRequestHandler<MarkWithdrawalPaidCommand, Result<WithdrawalRequestDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public MarkWithdrawalPaidCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<WithdrawalRequestDto>> Handle(MarkWithdrawalPaidCommand request, CancellationToken cancellationToken)
    {
        var withdrawal = await _db.WithdrawalRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(wr => wr.Id == request.WithdrawalId, cancellationToken);

        if (withdrawal is null)
        {
            return Failure("Withdrawal.NotFound", "Withdrawal request was not found.");
        }

        if (string.IsNullOrWhiteSpace(request.PayoutReference))
        {
            return Failure("Withdrawal.PayoutReferenceRequired", "Payout reference is required when marking a withdrawal as paid.");
        }

        if (!withdrawal.CanTransitionTo(WithdrawalStatus.Paid))
        {
            return Failure("Withdrawal.InvalidStatus", $"Only approved withdrawals can be marked as paid. Current status is {withdrawal.Status}.");
        }

        var now = DateTime.UtcNow;
        var payoutReference = request.PayoutReference.Trim();
        var payoutProvider = string.IsNullOrWhiteSpace(request.PayoutProvider)
            ? "Manual"
            : request.PayoutProvider.Trim();
        var adminNotes = string.IsNullOrWhiteSpace(request.AdminNotes)
            ? withdrawal.AdminNotes
            : request.AdminNotes.Trim();
        var reviewedAt = withdrawal.ReviewedAt ?? now;
        var reviewedByAdminId = withdrawal.ReviewedByAdminId ?? _currentUser.UserId;

        int transitionRows;
        try
        {
            transitionRows = await _db.WithdrawalRequests
                .Where(wr => wr.Id == withdrawal.Id && wr.Status == WithdrawalStatus.Approved)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(wr => wr.Status, WithdrawalStatus.Paid)
                        .SetProperty(wr => wr.PaidAt, now)
                        .SetProperty(wr => wr.PaidByAdminId, _currentUser.UserId)
                        .SetProperty(wr => wr.PayoutReference, payoutReference)
                        .SetProperty(wr => wr.PayoutProvider, payoutProvider)
                        .SetProperty(wr => wr.AdminNotes, adminNotes)
                        .SetProperty(wr => wr.ReviewedAt, reviewedAt)
                        .SetProperty(wr => wr.ReviewedByAdminId, reviewedByAdminId),
                    cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Failure("Withdrawal.PayoutReferenceConflict", "A withdrawal payout with the same reference already exists.");
        }

        if (transitionRows != 1)
        {
            return Failure("Withdrawal.StateConflict", "Withdrawal status changed before it could be marked as paid.");
        }

        WalletBalanceTrackingSync.SyncWithdrawal(_db, withdrawal.Id, wr =>
        {
            wr.Status = WithdrawalStatus.Paid;
            wr.PaidAt = now;
            wr.PaidByAdminId = _currentUser.UserId;
            wr.PayoutReference = payoutReference;
            wr.PayoutProvider = payoutProvider;
            wr.AdminNotes = adminNotes;
            wr.ReviewedAt = reviewedAt;
            wr.ReviewedByAdminId = reviewedByAdminId;
        });

        var updatedWithdrawal = await _db.WithdrawalRequests
            .AsNoTracking()
            .SingleAsync(wr => wr.Id == withdrawal.Id, cancellationToken);

        return Result<WithdrawalRequestDto>.Success(WithdrawalRequestDtoMapper.ToDto(updatedWithdrawal));
    }

    private static Result<WithdrawalRequestDto> Failure(string code, string message)
    {
        return Result<WithdrawalRequestDto>.Failure(new Error(code, message));
    }
}
