using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Events;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace AlMostashar.Application.Features.Payments.Commands.WebHook
{
    public class WebHookCommandHandler : IRequestHandler<WebHookCommand, Result<string>>
    {
        private readonly IAppDbContext _db;
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;
        private readonly IEscrowFundingService _escrowFundingService;
        private readonly ILogger<WebHookCommandHandler> _logger;

        public WebHookCommandHandler(
            IAppDbContext db,
            IMediator mediator,
            IConfiguration configuration,
            INotificationService notificationService,
            IEscrowFundingService escrowFundingService,
            ILogger<WebHookCommandHandler> logger)
        {
            _db = db;
            _mediator = mediator;
            _configuration = configuration;
            _notificationService = notificationService;
            _escrowFundingService = escrowFundingService;
            _logger = logger;
        }

        public async Task<Result<string>> Handle(WebHookCommand request, CancellationToken cancellationToken)
        {
            var obj = request.Payload.obj;

            // 1. Filter non-transaction webhooks
            if (request.Payload.type?.ToUpper() != "TRANSACTION")
            {
                await LogWebhookAsync(null, obj?.id, request.GatewayResponse, "Ignored", "Non-transaction webhook type.", cancellationToken);
                return Result<string>.Success("Webhook type ignored.");
            }

            // 2. HMAC Validation — timing-safe comparison with safe parsing
            if (!IsValidPayload(in obj, request.Hmac))
            {
                await LogWebhookAsync(null, obj?.id, request.GatewayResponse, "Error", "Invalid HMAC signature.", cancellationToken);
                return Result<string>.Failure(new Error("Webhook.InvalidSignature", "Invalid HMAC signature"));
            }

            // 3. Resolve Payment via Payment.Id from merchant_order_id (our special_reference)
            string? merchantOrderId = obj.order?.merchant_order_id;
            string paymentIdString = merchantOrderId?.Split('_').FirstOrDefault() ?? "";

            if (!int.TryParse(paymentIdString, out int paymentId))
            {
                await LogWebhookAsync(null, obj.id, request.GatewayResponse, "Error", $"Could not parse Payment.Id from merchant_order_id: {merchantOrderId}.", cancellationToken);
                return Result<string>.Failure(new Error("Webhook.InvalidPaymentId", "Could not parse Payment.Id from merchant_order_id."));
            }

            var payment = await _db.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

            if (payment is null)
            {
                await LogWebhookAsync(paymentId, obj.id, request.GatewayResponse, "Error", $"Payment record not found for Id={paymentId}.", cancellationToken);
                return Result<string>.Failure(new Error("Payment.NotFound", $"Payment record not found for Id={paymentId}."));
            }

            // 4. Determine incoming status from webhook fields
            PaymentStatus incomingStatus = DetermineIncomingStatus(obj);

            // 5. State machine — validate the transition
            if (incomingStatus == payment.Status)
            {
                // Duplicate webhook. If payment is already Succeeded, check for recovery needs.
                if (incomingStatus == PaymentStatus.Succeeded)
                {
                    await RecoverEscrowFundingIfNeeded(payment, cancellationToken);
                }

                await LogWebhookAsync(paymentId, obj.id, request.GatewayResponse, "Ignored", $"Duplicate webhook. Payment already in {payment.Status} state.", cancellationToken);
                return Result<string>.Success($"Payment already in {payment.Status} state.");
            }

            if (!IsValidTransition(payment.Status, incomingStatus))
            {
                await LogWebhookAsync(paymentId, obj.id, request.GatewayResponse, "Ignored", $"Invalid transition: {payment.Status} → {incomingStatus}.", cancellationToken);
                return Result<string>.Success($"Invalid state transition {payment.Status} → {incomingStatus}. Ignored.");
            }

            // 6. Update payment fields from the webhook data
            payment.TransactionId = obj.id;
            payment.ProviderOrderId = obj.order?.id.ToString();
            payment.PaymentMethod = obj.source_data?.type ?? payment.PaymentMethod;
            payment.GatewayResponse = request.GatewayResponse;

            DateTime.TryParse(obj.created_at, out DateTime payDate);
            payment.PayDate = payDate;

            // Accounting fields
            decimal amountEgp = obj.amount_cents / 100m;
            decimal providerFeeEgp = (obj.merchant_commission ?? (obj.amount_cents * 0.0275m + 300m)) / 100m;
            payment.ProviderFee = providerFeeEgp;
            payment.NetAmount = amountEgp - providerFeeEgp;

            // 7. Handle each transition
            string processingResult;

            if (incomingStatus == PaymentStatus.Succeeded && amountEgp != payment.Amount)
            {
                _logger.LogError(
                    "Webhook amount mismatch: Paymob amount={PaymobAmount} EGP, Payment.Amount={PaymentAmount} EGP for PaymentId={PaymentId}. Payment NOT marked as Succeeded.",
                    amountEgp, payment.Amount, payment.Id);

                // Save other fields (TransactionId, etc.) but DO NOT change Status
                await _db.SaveChangesAsync(cancellationToken);

                // Log exactly once and return safe success
                await LogWebhookAsync(paymentId, obj.id, request.GatewayResponse, "Error",
                    $"Amount mismatch: Paymob={amountEgp}, expected={payment.Amount}.", cancellationToken);

                return Result<string>.Success("Payment success skipped due to amount mismatch.");
            }

            payment.Status = incomingStatus;

            switch (incomingStatus)
            {
                case PaymentStatus.Succeeded:
                    processingResult = await HandleSucceededAsync(payment, obj, amountEgp, cancellationToken);
                    break;

                case PaymentStatus.Failed:
                    await _db.SaveChangesAsync(cancellationToken);
                    processingResult = "Failed";

                    // Notify client of failure
                    var failedInvoice = await _db.Invoices
                        .Include(i => i.ClientRequest)
                        .FirstOrDefaultAsync(i => i.Id == payment.InvoiceId, cancellationToken);
                    if (failedInvoice != null)
                    {
                        try
                        {
                            await _notificationService.NotifyPaymentStatusAsync(
                                failedInvoice.ClientRequest.ClientId,
                                failedInvoice.Id,
                                false,
                                cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to send payment failure notification for InvoiceId={InvoiceId}.", failedInvoice.Id);
                        }
                    }
                    break;

                case PaymentStatus.Refunded:
                    payment.RefundedAmount = (obj.refunded_amount_cents ?? 0m) / 100m;
                    await _db.SaveChangesAsync(cancellationToken);
                    processingResult = "Refunded";
                    break;

                default:
                    await _db.SaveChangesAsync(cancellationToken);
                    processingResult = "Succeeded";
                    break;
            }

            // 8. Log the webhook
            await LogWebhookAsync(paymentId, obj.id, request.GatewayResponse, processingResult, null, cancellationToken);

            return Result<string>.Success($"Payment updated to {incomingStatus}.");
        }

        /// <summary>
        /// Handles the Pending/Failed → Succeeded transition:
        /// updates invoice, publishes domain event, sends notification.
        /// Payment success is saved durably first; escrow funding failures are isolated.
        /// </summary>
        private async Task<string> HandleSucceededAsync(Payment payment, PaymobTransactionObjDto obj, decimal amountEgp, CancellationToken cancellationToken)
        {

            var invoice = await _db.Invoices
                .Include(i => i.ClientRequest)
                .FirstOrDefaultAsync(i => i.Id == payment.InvoiceId, cancellationToken);

            if (invoice is null)
            {
                _logger.LogError("Webhook: Invoice not found for PaymentId={PaymentId}, InvoiceId={InvoiceId}.", payment.Id, payment.InvoiceId);
                await _db.SaveChangesAsync(cancellationToken);
                return "Error";
            }

            // Idempotency guard: invoice already paid by a previous webhook delivery
            if (invoice.Status == InvoiceStatus.Paid)
            {
                // Save payment status update (in case it was pending→succeeded but invoice was already paid)
                await _db.SaveChangesAsync(cancellationToken);

                // Recover escrow if needed (idempotent)
                await RecoverEscrowFundingIfNeeded(payment, cancellationToken);

                return "Ignored";
            }

            // Update invoice state
            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidAt = DateTime.UtcNow;
            invoice.ClientRequest.Status = ClientRequestStatus.InProgress;

            // Save payment + invoice state durably FIRST
            await _db.SaveChangesAsync(cancellationToken);

            // Publish InvoicePaidEvent → triggers Case creation & Escrow funding
            try
            {
                    await _mediator.Publish(new InvoicePaidEvent(
                        invoiceId: invoice.Id,
                        clientRequestId: invoice.ClientRequestId,
                        paymentId: payment.Id,
                        transactionId: obj.id,
                        paymobOrderId: obj.order?.id ?? 0,
                        paidAmount: payment.Amount
                    ), cancellationToken);
            }
            catch (Exception ex)
            {
                // Do not throw — payment/invoice state is already saved.
                _logger.LogError(ex, "Failed to publish InvoicePaidEvent for PaymentId={PaymentId}, InvoiceId={InvoiceId}.", payment.Id, invoice.Id);
            }

            // Notify client via SignalR
            try
            {
                await _notificationService.NotifyPaymentStatusAsync(
                    invoice.ClientRequest.ClientId,
                    invoice.Id,
                    true,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send payment success notification for InvoiceId={InvoiceId}.", invoice.Id);
            }

            return "Succeeded";
        }

        /// <summary>
        /// Recovers escrow funding for a payment that is already Succeeded but may have
        /// missed its escrow funding (e.g. due to a crash after payment save).
        /// Uses the idempotent EscrowFundingService directly instead of republishing InvoicePaidEvent
        /// to avoid duplicate case creation.
        /// </summary>
        private async Task RecoverEscrowFundingIfNeeded(Payment payment, CancellationToken cancellationToken)
        {
            try
            {
                var invoice = await _db.Invoices
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.Id == payment.InvoiceId, cancellationToken);

                if (invoice is null || invoice.Status != InvoiceStatus.Paid)
                    return;

                var escrow = await _db.Escrows
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.RequestId == invoice.ClientRequestId, cancellationToken);

                // Only recover if escrow is missing or NotFunded
                if (escrow is null || escrow.Status == EscrowStatus.NotFunded)
                {
                    _logger.LogInformation(
                        "Webhook recovery: Escrow missing or NotFunded for PaymentId={PaymentId}, ClientRequestId={ClientRequestId}. Recovering...",
                        payment.Id, invoice.ClientRequestId);

                    await _escrowFundingService.FundEscrowAsync(
                        invoice.ClientRequestId,
                        payment.Id,
                        payment.Amount,
                        cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook recovery: Failed to recover escrow funding for PaymentId={PaymentId}.", payment.Id);
            }
        }

        /// <summary>
        /// Determines the incoming payment status from webhook fields.
        /// </summary>
        private static PaymentStatus DetermineIncomingStatus(PaymobTransactionObjDto obj)
        {
            if (obj.is_refunded) return PaymentStatus.Refunded;
            if (obj.success) return PaymentStatus.Succeeded;
            if (obj.pending) return PaymentStatus.Pending;
            return PaymentStatus.Failed;
        }

        /// <summary>
        /// Validates payment state transitions.
        /// Allowed: Pending→Succeeded, Pending→Failed, Failed→Succeeded, Succeeded→Refunded
        /// Blocked: Refunded→anything, Succeeded→Pending, Failed→Pending
        /// </summary>
        private static bool IsValidTransition(PaymentStatus current, PaymentStatus incoming)
        {
            return (current, incoming) switch
            {
                (PaymentStatus.Pending, PaymentStatus.Succeeded) => true,
                (PaymentStatus.Pending, PaymentStatus.Failed) => true,
                (PaymentStatus.Failed, PaymentStatus.Succeeded) => true,
                (PaymentStatus.Succeeded, PaymentStatus.Refunded) => true,
                _ => false
            };
        }
        /// <summary>
        /// Creates a WebhookLog entry for auditing.
        /// </summary>
        private async Task LogWebhookAsync(int? paymentId, int? transactionId, string? rawPayload, string processingResult, string? errorMessage, CancellationToken cancellationToken)
        {
            var log = new WebhookLog
            {
                PaymentId = paymentId,
                TransactionId = transactionId,
                RawPayload = rawPayload ?? string.Empty,
                ReceivedAt = DateTime.UtcNow,
                ProcessingResult = processingResult,
                ErrorMessage = errorMessage
            };
            _db.WebhookLogs.Add(log);
            await _db.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// HMAC validation using CryptographicOperations.FixedTimeEquals to prevent timing attacks.
        /// Safe against null/empty/malformed HMAC strings.
        /// </summary>
        private bool IsValidPayload(in PaymobTransactionObjDto obj, in string hmac)
        {
            // Safe HMAC parsing: return false for null/empty/malformed hmac
            if (string.IsNullOrWhiteSpace(hmac))
                return false;

            byte[] providedHash;
            try
            {
                providedHash = Convert.FromHexString(hmac);
            }
            catch (FormatException)
            {
                _logger.LogWarning("HMAC parsing failed: malformed hex string.");
                return false;
            }

            string hmacSecret = _configuration.GetValue<string>("PaymobSettings:HMAC") ?? "";
            string concatenatedString =
                $"{obj.amount_cents}" +
                $"{obj.created_at}" +
                $"{obj.currency}" +
                $"{obj.error_occured.ToString().ToLower()}" +
                $"{obj.has_parent_transaction.ToString().ToLower()}" +
                $"{obj.id}" +
                $"{obj.integration_id}" +
                $"{obj.is_3d_secure.ToString().ToLower()}" +
                $"{obj.is_auth.ToString().ToLower()}" +
                $"{obj.is_capture.ToString().ToLower()}" +
                $"{obj.is_refunded.ToString().ToLower()}" +
                $"{obj.is_standalone_payment.ToString().ToLower()}" +
                $"{obj.is_voided.ToString().ToLower()}" +
                $"{obj.order?.id}" +
                $"{obj.owner}" +
                $"{obj.pending.ToString().ToLower()}" +
                $"{obj.source_data?.pan}" +
                $"{obj.source_data?.sub_type}" +
                $"{obj.source_data?.type}" +
                $"{obj.success.ToString().ToLower()}";

            using var hmacSha512 = new HMACSHA512(Encoding.UTF8.GetBytes(hmacSecret));
            byte[] computedHash = hmacSha512.ComputeHash(Encoding.UTF8.GetBytes(concatenatedString));

            if (providedHash.Length != computedHash.Length)
                return false;

            return CryptographicOperations.FixedTimeEquals(computedHash, providedHash);
        }
    }
}
