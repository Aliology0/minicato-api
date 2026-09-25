using AlMostashar.Api.Helpers;
using AlMostashar.Application.Features.Escrows.Queries.GetRequestEscrow;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers;

[ApiController]
[Route("api/client/requests")]
[Authorize]
public class RequestEscrowsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RequestEscrowsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{requestId:int}/escrow")]
    public async Task<IActionResult> GetRequestEscrow(int requestId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetRequestEscrowQuery(requestId), cancellationToken);
        return this.ToActionResult(result);
    }
}
