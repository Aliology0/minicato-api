using AlMostashar.Api.Helpers;
using AlMostashar.Application.Features.Disputes.Commands.OpenDispute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers;

[Route("api/cases")]
[ApiController]
[Authorize(Roles = "Client,Lawyer")]
public class DisputesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DisputesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Opens a formal financial dispute for a given case.
    /// If an EscrowId is provided, the funds will be locked.
    /// </summary>
    [HttpPost("{caseId}/disputes")]
    public async Task<IActionResult> OpenDispute(
        int caseId,
        [FromForm] OpenDisputeCommand command,
        CancellationToken cancellationToken)
    {
        command.CaseId = caseId;
        var result = await _mediator.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }
}
