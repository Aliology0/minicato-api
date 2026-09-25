using AlMostashar.Application.Common.Models;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.ClientRequests.Commands.PayInvoice;

public record PayInvoiceCommand(int InvoiceId) : IRequest<Result<PaymentResultDto>>;
