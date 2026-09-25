using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Events;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Payments.Commands.SimulateInvoicePaid;

public sealed class SimulateInvoicePaidCommandHandler
    : IRequestHandler<SimulateInvoicePaidCommand, Result<SimulateInvoicePaidResponse>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IPublisher _publisher;

    public SimulateInvoicePaidCommandHandler(
        IAppDbContext db,
        ICurrentUserService currentUser,
        IPublisher publisher)
    {
        _db = db;
        _currentUser = currentUser;
        _publisher = publisher;
    }

    public async Task<Result<SimulateInvoicePaidResponse>> Handle(
        SimulateInvoicePaidCommand request,
        CancellationToken cancellationToken)
    {
        var invoice = await _db.Invoices
            .Include(value => value.ClientRequest)
            .Include(value => value.Payment)
            .SingleOrDefaultAsync(value => value.Id == request.InvoiceId, cancellationToken);
        if (invoice is null)
            return Result<SimulateInvoicePaidResponse>.Failure(new Error("Invoice.NotFound", "Invoice was not found."));
        if (invoice.ClientRequest.ClientId != _currentUser.UserId)
            return Result<SimulateInvoicePaidResponse>.Failure(new Error("Invoice.Forbidden", "Only the invoice client can simulate payment."));

        var existingCaseId = await _db.CaseClientRequests.AsNoTracking()
            .Where(value => value.ClientRequestId == invoice.ClientRequestId)
            .Select(value => (int?)value.CaseId)
            .SingleOrDefaultAsync(cancellationToken);
        if (invoice.Status == InvoiceStatus.Paid)
            return Result<SimulateInvoicePaidResponse>.Success(new(
                invoice.Id, invoice.ClientRequestId, invoice.Payment?.Id ?? 0, existingCaseId, true));

        var lawyerId = invoice.ClientRequest.LawyerServiceLawyerId;
        if (!lawyerId.HasValue)
            return Result<SimulateInvoicePaidResponse>.Failure(new Error("Invoice.MissingLawyer", "The request has no accepted lawyer."));

        var payment = invoice.Payment ?? new Payment
        {
            InvoiceId = invoice.Id,
            Amount = invoice.TotalAmount,
            Currency = "EGP",
            PayDate = DateTime.UtcNow,
            Status = PaymentStatus.Succeeded,
            PaymentMethod = "LocalDevelopmentSimulation",
            PaymentProvider = PaymentProvider.Paymob,
            TransactionId = 1_000_000 + invoice.Id,
            ProviderOrderId = (2_000_000 + invoice.Id).ToString()
        };
        if (invoice.Payment is null)
            _db.Payments.Add(payment);
        else
        {
            payment.Status = PaymentStatus.Succeeded;
            payment.PayDate = DateTime.UtcNow;
        }
        invoice.Status = InvoiceStatus.Paid;
        invoice.PaidAt = DateTime.UtcNow;
        invoice.ClientRequest.Status = ClientRequestStatus.InProgress;
        await _db.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new InvoicePaidEvent(
            invoice.Id,
            invoice.ClientRequestId,
            payment.Id,
            payment.TransactionId ?? 0,
            int.TryParse(payment.ProviderOrderId, out var orderId) ? orderId : 0,
            payment.Amount), cancellationToken);

        var caseId = await _db.CaseClientRequests.AsNoTracking()
            .Where(value => value.ClientRequestId == invoice.ClientRequestId)
            .Select(value => (int?)value.CaseId)
            .SingleOrDefaultAsync(cancellationToken);
        return Result<SimulateInvoicePaidResponse>.Success(new(
            invoice.Id, invoice.ClientRequestId, payment.Id, caseId, false));
    }
}
