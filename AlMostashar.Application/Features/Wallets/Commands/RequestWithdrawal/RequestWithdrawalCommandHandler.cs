using System.Globalization;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Wallets.DTOs;
using AlMostashar.Application.Features.Wallets.Helpers;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Wallets.Commands.RequestWithdrawal;

public class RequestWithdrawalCommandHandler : IRequestHandler<RequestWithdrawalCommand, Result<WithdrawalRequestDto>>
{
    private const string WithdrawalReferenceType = "Withdrawal";
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ISensitiveDataProtector _sensitiveDataProtector;

    public RequestWithdrawalCommandHandler(
        IAppDbContext db,
        ICurrentUserService currentUser,
        ISensitiveDataProtector sensitiveDataProtector)
    {
        _db = db;
        _currentUser = currentUser;
        _sensitiveDataProtector = sensitiveDataProtector;
    }

    public async Task<Result<WithdrawalRequestDto>> Handle(RequestWithdrawalCommand request, CancellationToken cancellationToken)
    {
        Result<WithdrawalRequestDto>? result = null;

        await _db.ExecuteInTransactionAsync(async ct =>
        {
            var lawyerId = await _db.Lawyers
                .Where(l => l.Id == _currentUser.UserId)
                .Select(l => (int?)l.Id)
                .FirstOrDefaultAsync(ct);

            if (!lawyerId.HasValue)
            {
                result = Failure("Lawyer.Auth.Unauthorized", "Authenticated user is not a lawyer.");
                return;
            }

            var wallet = await _db.Wallets
                .AsNoTracking()
                .Where(w => w.LawyerId == lawyerId.Value)
                .Select(w => new { w.Id })
                .FirstOrDefaultAsync(ct);

            if (wallet is null)
            {
                result = Failure("Wallet.NotFound", "Lawyer wallet was not found.");
                return;
            }

            if (request.Amount <= 0)
            {
                result = Failure("Withdrawal.InvalidAmount", "Withdrawal amount must be greater than zero.");
                return;
            }

            var accountDetails = request.AccountDetails.Trim();
            var protectedAccountDetails = _sensitiveDataProtector.Protect(accountDetails);
            var maskedAccountDetails = _sensitiveDataProtector.Mask(accountDetails);

            var reserveRows = await _db.Wallets
                .Where(w => w.Id == wallet.Id && w.AvailableBalance >= request.Amount)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        w => w.AvailableBalance,
                        w => w.AvailableBalance - request.Amount),
                    ct);

            if (reserveRows != 1)
            {
                result = Failure("Withdrawal.InsufficientBalance", "Withdrawal amount cannot exceed available balance.");
                return;
            }

            var balanceAfter = await _db.Wallets
                .AsNoTracking()
                .Where(w => w.Id == wallet.Id)
                .Select(w => w.AvailableBalance)
                .SingleAsync(ct);

            WalletBalanceTrackingSync.SyncAvailableBalance(_db, wallet.Id, balanceAfter);

            var now = DateTime.UtcNow;
            var withdrawal = new WithdrawalRequest
            {
                LawyerId = lawyerId.Value,
                WalletId = wallet.Id,
                Amount = request.Amount,
                Method = request.Method,
                AccountDetailsEncrypted = protectedAccountDetails,
                AccountDetailsMasked = maskedAccountDetails,
                Status = WithdrawalStatus.Pending,
                RequestedAt = now
            };

            _db.WithdrawalRequests.Add(withdrawal);
            await _db.SaveChangesAsync(ct);

            var transaction = new WalletTransaction
            {
                WalletId = wallet.Id,
                Amount = request.Amount,
                Type = TransactionType.Debit,
                ReferenceType = WithdrawalReferenceType,
                ReferenceId = withdrawal.Id.ToString(CultureInfo.InvariantCulture),
                WithdrawalRequestId = withdrawal.Id,
                Description = $"Withdrawal request #{withdrawal.Id} reserved",
                BalanceAfter = balanceAfter,
                CreatedAt = now
            };

            _db.WalletTransactions.Add(transaction);
            await _db.SaveChangesAsync(ct);

            withdrawal.WalletTransactionId = transaction.Id;
            await _db.SaveChangesAsync(ct);

            result = Result<WithdrawalRequestDto>.Success(WithdrawalRequestDtoMapper.ToDto(withdrawal));
        }, cancellationToken);

        return result!;
    }

    private static Result<WithdrawalRequestDto> Failure(string code, string message)
    {
        return Result<WithdrawalRequestDto>.Failure(new Error(code, message));
    }
}
