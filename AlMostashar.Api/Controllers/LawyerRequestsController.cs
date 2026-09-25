using AlMostashar.Api.Helpers;
using AlMostashar.Application.Features.ClientRequests.Commands.AcceptRequest;
using AlMostashar.Application.Features.ClientRequests.Commands.RejectRequest;
using AlMostashar.Application.Features.ClientRequests.Commands.SendOffer;
using AlMostashar.Application.Features.ClientRequests.Queries.GetBroadcastRequests;
using AlMostashar.Application.Features.ClientRequests.Queries.GetLawyerDirectRequests;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers;

[ApiController]
[Route("api/lawyer/requests")]
[Authorize(Roles = "Lawyer")]
public class LawyerRequestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LawyerRequestsController(IMediator mediator) => _mediator = mediator;

    // ─── Commands ─────────────────────────────────────────────────────────

    /// <summary>Accept a request.</summary>
    [HttpPut("{id:int}/accept")]
    public async Task<IActionResult> AcceptRequest(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new AcceptRequestCommand(id), cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>Reject a request.</summary>
    [HttpPut("{id:int}/reject")]
    public async Task<IActionResult> RejectRequest(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RejectRequestCommand(id), cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>Send an offer for a request (broadcast or assigned direct request).</summary>
    [HttpPost("{id:int}/offers")]
    public async Task<IActionResult> SendOffer(
        int id,
        [FromBody] SendOfferRequest body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SendOfferCommand(id, body.LegalServiceId, body.OfferedAmount, body.Note),
            cancellationToken);
        return this.ToActionResult(result);
    }

    // ─── Queries ──────────────────────────────────────────────────────────

    /// <summary>Get available broadcast requests (Pending only).</summary>
    [HttpGet("broadcast/available")]
    public async Task<IActionResult> GetBroadcastRequests(
        [FromQuery] decimal? minBudget,
        [FromQuery] decimal? maxBudget,
        [FromQuery] ServiceType? serviceType,
        [FromQuery] int? governorateId,
        [FromQuery] int? cityId,
        [FromQuery] int? cursor,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetBroadcastRequestsQuery(minBudget, maxBudget, serviceType, governorateId, cityId, cursor, pageSize), cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>Get incoming direct requests.</summary>
    [HttpGet("direct/incoming")]
    public async Task<IActionResult> GetLawyerDirectRequests(
        [FromQuery] ClientRequestStatus? status,
        [FromQuery] ServiceType? serviceType,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetLawyerDirectRequestsQuery(status, serviceType), cancellationToken);
        return this.ToActionResult(result);
    }

}

public record SendOfferRequest(int? LegalServiceId, decimal OfferedAmount, string? Note);
