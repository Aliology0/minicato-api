namespace AlMostashar.Application.Common.Interfaces;

/// <summary>
/// Idempotent service for funding escrows from validated payment success signals.
/// Must NOT perform Paymob/HMAC/provider validation.
/// Must NOT credit wallet or create WalletTransactions.
/// Must NOT manage its own transaction boundary.
/// </summary>
public interface IEscrowFundingService
{
    /// <summary>
    /// Funds an escrow for the given client request. Idempotent: safe to call multiple times
    /// with the same parameters. Logs inconsistencies without throwing for business-level issues.
    /// </summary>
    Task FundEscrowAsync(int clientRequestId, int paymentId, decimal paidAmount, CancellationToken cancellationToken);
}
