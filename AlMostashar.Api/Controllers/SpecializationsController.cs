using AlMostashar.Api.Helpers;
using AlMostashar.Application.Features.Specializations.Queries.GetSpecializations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers;

[ApiController]
[Route("api/specializations")]
[AllowAnonymous]
public class SpecializationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SpecializationsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetSpecializations(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSpecializationsQuery(), cancellationToken);
        return this.ToActionResult(result);
    }
}
