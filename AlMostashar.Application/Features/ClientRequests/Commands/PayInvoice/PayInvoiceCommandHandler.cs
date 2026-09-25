using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Models;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.ClientRequests.Commands.PayInvoice;

public class PayInvoiceCommandHandler : IRequestHandler<PayInvoiceCommand, Result<PaymentResultDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentService _paymentService;

    public PayInvoiceCommandHandler(IAppDbContext db, ICurrentUserService currentUser, IPaymentService paymentService)
    {
        _db = db;
        _currentUser = currentUser;
        _paymentService = paymentService;
    }

    public async Task<Result<PaymentResultDto>> Handle(PayInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _db.Invoices
            .Select(i=> new {i.Id,
                i.TotalAmount,
                i.Status,
                ClientRequestStatus=i.ClientRequest.Status,
                i.ClientRequest.ClientId,
                ClientFirstName=i.ClientRequest.Client.FirstName,
                ClientLastName= i.ClientRequest.Client.LastName,
                ClientEmail= i.ClientRequest.Client.Email,
                ServiceTitle= i.ClientRequest.RequestedLegalService!.Title,
                ServiceDescription=i.ClientRequest.RequestedLegalService.Summary,
            })
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);
        
        if (invoice is null)
            return Result<PaymentResultDto>.Failure(
                new Error("Invoice.NotFound", Messages.Generic.NotFound("Invoice")));

        // Validate ownership
        if (invoice.ClientId != _currentUser.UserId)
            return Result<PaymentResultDto>.Failure(
                new Error("Invoice.Unauthorized", Messages.Invoices.Unauthorized));

        if (invoice.Status == InvoiceStatus.Paid)
            return Result<PaymentResultDto>.Failure(
                new Error("Invoice.AlreadyPaid", Messages.Invoices.AlreadyPaid));

        // Validate request is accepted before allowing payment
        if (invoice.ClientRequestStatus != ClientRequestStatus.Accepted)
            return Result<PaymentResultDto>.Failure(
                new Error("Invoice.RequestNotAccepted", Messages.Invoices.RequestNotAccepted));

        // --- Payment record management ---
        var payment = await _db.Payments
            .FirstOrDefaultAsync(p => p.InvoiceId == invoice.Id, cancellationToken);

        if (payment != null)
        {
            // Refunded payments are terminal — cannot retry
            if (payment.Status == PaymentStatus.Refunded)
                return Result<PaymentResultDto>.Failure(
                    new Error("Payment.Refunded", "This payment was refunded and cannot be retried."));

            // Already succeeded — should not happen if invoice status check above works, but guard anyway
            if (payment.Status == PaymentStatus.Succeeded)
                return Result<PaymentResultDto>.Failure(
                    new Error("Invoice.AlreadyPaid", Messages.Invoices.AlreadyPaid));

            // Status is Pending or Failed — we will reuse this payment record.
            // Do NOT reset to Pending yet — wait until Paymob API call succeeds.
        }
        else
        {
            // Create new Payment record with Pending status
            payment = new Payment
            {
                InvoiceId = invoice.Id,
                Amount = invoice.TotalAmount,
                Currency = "EGP",
                PayDate = DateTime.UtcNow,
                Status = PaymentStatus.Pending,
                PaymentProvider = PaymentProvider.Paymob
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync(cancellationToken); // Persist to get Payment.Id
        }

        // --- Create Paymob intention ---
        var result = await _paymentService.CreatePaymentIntentionAsync(
            payment.Id,
            invoice.TotalAmount,
            3600 * 24,
            invoice.ServiceTitle,
            invoice.ServiceDescription,
            invoice.ClientFirstName,
            invoice.ClientLastName,
            invoice.ClientEmail,
            "NA");

        // Paymob API call succeeded — now safe to update the payment record
        if (payment.Status == PaymentStatus.Failed)
        {
            // Clear old gateway fields from the previous failed attempt
            payment.TransactionId = null;
            payment.ProviderOrderId = null;
            payment.PaymentMethod = null;
            payment.GatewayResponse = null;
            payment.RefundedAmount = 0;
            payment.Status = PaymentStatus.Pending;
        }

        payment.IntentionId = result.IntentionId;
        await _db.SaveChangesAsync(cancellationToken);

        return Result<PaymentResultDto>.Success(result);
    }
}
