using AlMostashar.Api.Helpers;
using AlMostashar.Application.Features.Lookups.Queries.GetCities;
using AlMostashar.Application.Features.Lookups.Queries.GetGovernorates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers;

[ApiController]
[Route("api/lookups")]
[AllowAnonymous]
public class LookupsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LookupsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("governorates")]
    public async Task<IActionResult> GetGovernorates(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetGovernoratesQuery(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("cities")]
    public async Task<IActionResult> GetCities([FromQuery] int governorateId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCitiesQuery(governorateId), cancellationToken);
        return this.ToActionResult(result);
    }
}
