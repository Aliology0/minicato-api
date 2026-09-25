using AlMostashar.Api.Helpers;
using AlMostashar.Application.Features.Escrows.Commands.MarkEscrowAsDisputed;
using AlMostashar.Application.Features.Escrows.Commands.RefundEscrow;
using AlMostashar.Application.Features.Escrows.Commands.ReleaseEscrow;
using AlMostashar.Application.Features.Escrows.Queries.GetEscrowById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers;

[ApiController]
[Route("api/admin/escrows")]
[Authorize(Roles = "Admin")]
public class AdminEscrowsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminEscrowsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{escrowId:int}")]
    public async Task<IActionResult> GetEscrow(int escrowId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetEscrowByIdQuery(escrowId), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{escrowId:int}/release")]
    public async Task<IActionResult> ReleaseEscrow(int escrowId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ReleaseEscrowCommand(escrowId), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{escrowId:int}/refund")]
    public async Task<IActionResult> RefundEscrow(int escrowId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RefundEscrowCommand(escrowId), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{escrowId:int}/mark-disputed")]
    public async Task<IActionResult> MarkEscrowAsDisputed(int escrowId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new MarkEscrowAsDisputedCommand(escrowId), cancellationToken);
        return this.ToActionResult(result);
    }
}
