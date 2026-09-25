using AlMostashar.Api.Helpers;
using AlMostashar.Application.Features.Wallets.Commands.ApproveWithdrawal;
using AlMostashar.Application.Features.Wallets.Commands.MarkWithdrawalPaid;
using AlMostashar.Application.Features.Wallets.Commands.RejectWithdrawal;
using AlMostashar.Application.Features.Wallets.Queries.GetAdminWithdrawalById;
using AlMostashar.Application.Features.Wallets.Queries.GetAdminWithdrawals;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers;

[ApiController]
[Route("api/admin/withdrawals")]
[Authorize(Roles = "Admin")]
public class AdminWithdrawalsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminWithdrawalsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetWithdrawals(
        [FromQuery] WithdrawalStatus? status,
        [FromQuery] int? cursor,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetAdminWithdrawalsQuery(status, cursor, pageSize),
            cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetWithdrawal(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAdminWithdrawalByIdQuery(id), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> ApproveWithdrawal(
        int id,
        [FromBody] ApproveWithdrawalCommand command,
        CancellationToken cancellationToken)
    {
        command.WithdrawalId = id;
        var result = await _mediator.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> RejectWithdrawal(
        int id,
        [FromBody] RejectWithdrawalCommand command,
        CancellationToken cancellationToken)
    {
        command.WithdrawalId = id;
        var result = await _mediator.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{id:int}/mark-paid")]
    public async Task<IActionResult> MarkWithdrawalPaid(
        int id,
        [FromBody] MarkWithdrawalPaidCommand command,
        CancellationToken cancellationToken)
    {
        command.WithdrawalId = id;
        var result = await _mediator.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }
}
