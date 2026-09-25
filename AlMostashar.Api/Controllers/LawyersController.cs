using AlMostashar.Api.Helpers;
using AlMostashar.Application.Features.Lawyers.Commands.UpdateLawyerProfile;
using AlMostashar.Application.Features.Lawyers.Queries.GetLawyerProfile;
using AlMostashar.Application.Features.Lawyers.Queries.GetLawyers;
using AlMostashar.Application.Features.Lawyers.Queries.GetLawyerHomeAnalytics;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AlMostashar.Application.Features.Lawyers.Queries.GetLawyerSpecializations;

namespace AlMostashar.Api.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "Lawyer")]
[ApiController]
public class LawyersController : ControllerBase
{
    private readonly IMediator _mediator;

    public LawyersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetLawyers([FromQuery] GetLawyersQuery lawyersQuery, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(lawyersQuery);
        return this.ToActionResult(result);
    }
    [AllowAnonymous]
    /// <summary>Get public profile of an active/verified lawyer.</summary>
    [HttpGet("{lawyerId:int}/profile")]
    public async Task<IActionResult> GetLawyerProfile(
        int lawyerId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetLawyerProfileQuery(lawyerId), cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>Lawyer updates their own profile information.</summary>
    [HttpPut("edit-profile")]
    public async Task<IActionResult> UpdateLawyerProfile(
        [FromForm] UpdateLawyerProfileCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>Get dashboard analytics for the authenticated lawyer.</summary>
    [HttpGet("home/analytics")]
    public async Task<IActionResult> GetLawyerHomeAnalytics(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLawyerHomeAnalyticsQuery(), cancellationToken);
        return this.ToActionResult(result);
    }
    [AllowAnonymous]
    [HttpGet("specializations")]
    public async Task<IActionResult> GetLawyerSpecializations(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLawyerSpecializationsQuery(), cancellationToken);
        return this.ToActionResult(result);
    }

}
