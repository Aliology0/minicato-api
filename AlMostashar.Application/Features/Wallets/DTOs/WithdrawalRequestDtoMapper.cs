using AlMostashar.Domain.Entities;

namespace AlMostashar.Application.Features.Wallets.DTOs;

public static class WithdrawalRequestDtoMapper
{
    public static WithdrawalRequestDto ToDto(
        WithdrawalRequest withdrawal,
        string? accountDetailsFull = null,
        string? lawyerName = null,
        string? lawyerEmail = null)
    {
        return new WithdrawalRequestDto
        {
            Id = withdrawal.Id,
            LawyerId = withdrawal.LawyerId,
            WalletId = withdrawal.WalletId,
            Amount = withdrawal.Amount,
            Method = withdrawal.Method,
            AccountDetails = withdrawal.AccountDetailsMasked,
            AccountDetailsMasked = withdrawal.AccountDetailsMasked,
            AccountDetailsFull = accountDetailsFull,
            Status = withdrawal.Status,
            RequestedAt = withdrawal.RequestedAt,
            ReviewedAt = withdrawal.ReviewedAt,
            ReviewedByAdminId = withdrawal.ReviewedByAdminId,
            AdminNotes = withdrawal.AdminNotes,
            RejectionReason = withdrawal.RejectionReason,
            PayoutReference = withdrawal.PayoutReference,
            PayoutProvider = withdrawal.PayoutProvider,
            PaidByAdminId = withdrawal.PaidByAdminId,
            PaidAt = withdrawal.PaidAt,
            WalletTransactionId = withdrawal.WalletTransactionId,
            LawyerName = lawyerName,
            LawyerEmail = lawyerEmail
        };
    }
}
