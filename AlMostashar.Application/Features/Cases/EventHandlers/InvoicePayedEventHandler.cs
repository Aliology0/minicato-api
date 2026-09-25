using AlMostashar.Application.Features.Cases.Commands.CreateCaseFromClientRequest;
using AlMostashar.Domain.Events;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Application.Features.Cases.EventHandlers;

public sealed class InvoicePaidEventHandler : INotificationHandler<InvoicePaidEvent>
{
    private readonly ISender _sender;
    private readonly ILogger<InvoicePaidEventHandler> _logger;

    public InvoicePaidEventHandler(ISender sender, ILogger<InvoicePaidEventHandler> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Handle(InvoicePaidEvent notification, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateCaseFromClientRequestCommand(notification.ClientRequestId),
            cancellationToken);

        if (!result.IsSuccess)
            _logger.LogError(
                "Case creation failed for paid invoice {InvoiceId}, request {ClientRequestId}: {Error}",
                notification.InvoiceId, notification.ClientRequestId, result.Error?.Message);
    }
}
