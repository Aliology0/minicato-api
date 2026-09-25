using AlMostashar.Api.Helpers;
using AlMostashar.Application.Features.ClientHome.Queries.GetClientHomeAnalytics;
using AlMostashar.Application.Features.ClientHome.Queries.GetRecentCaseUpdates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers;

[ApiController]
[Route("api/client/home")]
[Authorize(Roles = "Client")]
public class ClientHomeController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientHomeController(IMediator mediator) => _mediator = mediator;

    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics()
    {
        var query = new GetClientHomeAnalyticsQuery();
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("recent-updates")]
    public async Task<IActionResult> GetRecentUpdates([FromQuery] int limit = 5)
    {
        var query = new GetRecentCaseUpdatesQuery { Limit = limit };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
