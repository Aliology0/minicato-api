using AlMostashar.Api.Helpers;
using AlMostashar.Application.Features.LawyerHome.Queries.GetLawyerActiveCasesSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers;

[ApiController]
[Route("api/lawyer/home")]
[Authorize(Roles = "Lawyer")]
public class LawyerHomeController : ControllerBase
{
    private readonly IMediator _mediator;

    public LawyerHomeController(IMediator mediator) => _mediator = mediator;

    [HttpGet("active-cases")]
    public async Task<IActionResult> GetActiveCases([FromQuery] int limit = 5)
    {
        var result = await _mediator.Send(new GetLawyerActiveCasesSummaryQuery { Limit = limit });

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
