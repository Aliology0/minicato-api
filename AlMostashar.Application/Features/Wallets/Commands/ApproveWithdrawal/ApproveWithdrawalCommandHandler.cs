using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Wallets.DTOs;
using AlMostashar.Application.Features.Wallets.Helpers;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Wallets.Commands.ApproveWithdrawal;

public class ApproveWithdrawalCommandHandler : IRequestHandler<ApproveWithdrawalCommand, Result<WithdrawalRequestDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ApproveWithdrawalCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<WithdrawalRequestDto>> Handle(ApproveWithdrawalCommand request, CancellationToken cancellationToken)
    {
        var withdrawal = await _db.WithdrawalRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(wr => wr.Id == request.WithdrawalId, cancellationToken);

        if (withdrawal is null)
        {
            return Failure("Withdrawal.NotFound", "Withdrawal request was not found.");
        }

        if (!withdrawal.CanTransitionTo(WithdrawalStatus.Approved))
        {
            return Failure("Withdrawal.InvalidStatus", $"Only pending withdrawals can be approved. Current status is {withdrawal.Status}.");
        }

        var now = DateTime.UtcNow;
        var transitionRows = await _db.WithdrawalRequests
            .Where(wr => wr.Id == withdrawal.Id && wr.Status == WithdrawalStatus.Pending)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(wr => wr.Status, WithdrawalStatus.Approved)
                    .SetProperty(wr => wr.ReviewedAt, now)
                    .SetProperty(wr => wr.ReviewedByAdminId, _currentUser.UserId)
                    .SetProperty(wr => wr.AdminNotes, request.AdminNotes),
                cancellationToken);

        if (transitionRows != 1)
        {
            return Failure("Withdrawal.StateConflict", "Withdrawal status changed before approval could be applied.");
        }

        WalletBalanceTrackingSync.SyncWithdrawal(_db, withdrawal.Id, wr =>
        {
            wr.Status = WithdrawalStatus.Approved;
            wr.ReviewedAt = now;
            wr.ReviewedByAdminId = _currentUser.UserId;
            wr.AdminNotes = request.AdminNotes;
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
