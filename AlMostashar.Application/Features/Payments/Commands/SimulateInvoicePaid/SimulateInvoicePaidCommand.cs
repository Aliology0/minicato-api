using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Payments.Commands.SimulateInvoicePaid;

public sealed record SimulateInvoicePaidCommand(int InvoiceId)
    : IRequest<Result<SimulateInvoicePaidResponse>>;

public sealed record SimulateInvoicePaidResponse(
    int InvoiceId,
    int ClientRequestId,
    int PaymentId,
    int? CaseId,
    bool AlreadyPaid);
