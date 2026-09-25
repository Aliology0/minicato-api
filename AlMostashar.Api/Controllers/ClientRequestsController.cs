using AlMostashar.Api.Helpers;
using AlMostashar.Application.Features.ClientRequests.Commands.AcceptOffer;
using AlMostashar.Application.Features.ClientRequests.Commands.CancelRequest;
using AlMostashar.Application.Features.ClientRequests.Commands.CreateBroadcastRequest;
using AlMostashar.Application.Features.ClientRequests.Commands.CreateDirectRequest;
using AlMostashar.Application.Features.ClientRequests.Commands.PayInvoice;
using AlMostashar.Application.Features.ClientRequests.Commands.RejectOffer;
using AlMostashar.Application.Features.ClientRequests.Queries.GetClientOffers;
using AlMostashar.Application.Features.ClientRequests.Queries.GetLawyersByService;
using AlMostashar.Application.Features.ClientRequests.Queries.GetMyRequests;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers;

[ApiController]
[Route("api/client/requests")]
[Authorize(Roles = "Client")]
public class ClientRequestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientRequestsController(IMediator mediator) => _mediator = mediator;

    // ─── Commands ─────────────────────────────────────────────────────────

    /// <summary>Create a direct request to a specific lawyer.</summary>
    [HttpPost("direct")]
    public async Task<IActionResult> CreateDirectRequest(
        [FromBody] CreateDirectRequestCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>Create a broadcast request to all lawyers.</summary>
    [HttpPost("broadcast")]
    public async Task<IActionResult> CreateBroadcastRequest(
        [FromBody] CreateBroadcastRequestCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>Cancel a pending request.</summary>
    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> CancelRequest(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CancelRequestCommand(id), cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>Pay invoice (dummy payment).</summary>
    [HttpPost("invoices/{invoiceId:int}/pay")]
    public async Task<IActionResult> PayInvoice(int invoiceId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new PayInvoiceCommand(invoiceId), cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>Accept a lawyer offer.</summary>
    [HttpPut("offers/{offerId:int}/accept")]
    public async Task<IActionResult> AcceptOffer(int offerId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AcceptOfferCommand(offerId), cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>Reject a lawyer offer.</summary>
    [HttpPut("offers/{offerId:int}/reject")]
    public async Task<IActionResult> RejectOffer(int offerId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RejectOfferCommand(offerId), cancellationToken);
        return this.ToActionResult(result);
    }

    // ─── Queries ──────────────────────────────────────────────────────────

    /// <summary>Get all client requests (paginated).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAllRequests(
        [FromQuery] ClientRequestStatus? status,
        [FromQuery] ServiceType? serviceType,
        [FromQuery] int? cursor,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMyRequestsQuery(status, serviceType, cursor, pageSize), cancellationToken);
        return this.ToActionResult(result);
    }


    /// <summary>Get lawyers offering a specific service with their prices.</summary>
    [HttpGet("lawyers-by-service/{serviceId:int}")]
    public async Task<IActionResult> GetLawyersByService(
        int serviceId,
        [FromQuery] int? governorateId,
        [FromQuery] int? cityId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetLawyersByServiceQuery(serviceId, governorateId, cityId), cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>Get all offers for my requests (paginated). Optional filter by requestId.</summary>
    [HttpGet("offers")]
    public async Task<IActionResult> GetOffers(
        [FromQuery] GetClientOffersQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return this.ToActionResult(result);
    }
}
