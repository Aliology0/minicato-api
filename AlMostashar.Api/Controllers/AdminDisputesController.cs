using AlMostashar.Api.Helpers;
using AlMostashar.Application.Features.Disputes.Commands.ResolveDispute;
using AlMostashar.Application.Features.Disputes.Queries.GetDisputeDetails;
using AlMostashar.Application.Features.Disputes.Queries.GetDisputesList;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers;

[Route("api/admin/disputes")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminDisputesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminDisputesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets a cursor-paginated list of disputes.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDisputesList(
        [FromQuery] GetDisputesListQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>
    /// Gets the full details of a specific dispute.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDisputeDetails(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDisputeDetailsQuery(id), cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>
    /// Resolves a dispute, optionally releasing or refunding escrow funds.
    /// </summary>
    [HttpPost("{id}/resolve")]
    public async Task<IActionResult> ResolveDispute(
        int id,
        [FromBody] ResolveDisputeCommand command,
        CancellationToken cancellationToken)
    {
        command.DisputeId = id;
        var result = await _mediator.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }
}
