using AlMostashar.Api.Helpers;
using AlMostashar.Application.Features.ClientProfile.Commands.ChangePassword;
using AlMostashar.Application.Features.ClientProfile.Commands.EditProfile;
using AlMostashar.Application.Features.ClientProfile.Queries.GetProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers;

[ApiController]
[Route("api/client/profile")]
[Authorize(Roles = "Client")]
public class ClientProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetClientProfileQuery(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> EditProfile([FromForm] EditClientProfileCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }
}
