using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Application.Features.Escrows.EventHandlers;

/// <summary>
/// Reacts to InvoicePaidEvent by funding the escrow via the idempotent EscrowFundingService.
/// Does NOT query Payment to get amount — uses notification.PaidAmount directly.
/// Does NOT throw for normal business inconsistencies.
/// </summary>
public sealed class FundEscrowOnInvoicePaidEventHandler : INotificationHandler<InvoicePaidEvent>
{
    private readonly IEscrowFundingService _escrowFundingService;
    private readonly ILogger<FundEscrowOnInvoicePaidEventHandler> _logger;

    public FundEscrowOnInvoicePaidEventHandler(
        IEscrowFundingService escrowFundingService,
        ILogger<FundEscrowOnInvoicePaidEventHandler> logger)
    {
        _escrowFundingService = escrowFundingService;
        _logger = logger;
    }

    public async Task Handle(InvoicePaidEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "FundEscrowOnInvoicePaid: Handling InvoicePaidEvent for InvoiceId={InvoiceId}, ClientRequestId={ClientRequestId}, PaymentId={PaymentId}, PaidAmount={PaidAmount}.",
            notification.InvoiceId, notification.ClientRequestId, notification.PaymentId, notification.PaidAmount);

        try
        {
            await _escrowFundingService.FundEscrowAsync(
                notification.ClientRequestId,
                notification.PaymentId,
                notification.PaidAmount,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Do not throw — escrow funding failure must not roll back already saved payment/invoice state.
            _logger.LogError(ex,
                "FundEscrowOnInvoicePaid: Failed to fund escrow for ClientRequestId={ClientRequestId}, PaymentId={PaymentId}. Escrow may need manual recovery.",
                notification.ClientRequestId, notification.PaymentId);
        }
    }
}
