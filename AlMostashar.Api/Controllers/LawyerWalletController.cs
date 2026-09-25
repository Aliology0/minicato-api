using AlMostashar.Api.Helpers;
using AlMostashar.Application.Features.Wallets.Commands.CancelWithdrawal;
using AlMostashar.Application.Features.Wallets.Commands.RequestWithdrawal;
using AlMostashar.Application.Features.Wallets.Queries.GetMyWallet;
using AlMostashar.Application.Features.Wallets.Queries.GetMyWalletTransactions;
using AlMostashar.Application.Features.Wallets.Queries.GetMyWithdrawals;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers;

[ApiController]
[Route("api/lawyer/wallet")]
[Authorize(Roles = "Lawyer")]
public class LawyerWalletController : ControllerBase
{
    private readonly IMediator _mediator;

    public LawyerWalletController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyWallet(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyWalletQuery(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetMyWalletTransactions(
        [FromQuery] int? cursor,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMyWalletTransactionsQuery(cursor, pageSize),
            cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("withdrawals")]
    public async Task<IActionResult> RequestWithdrawal(
        [FromBody] RequestWithdrawalCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("withdrawals")]
    public async Task<IActionResult> GetMyWithdrawals(
        [FromQuery] int? cursor,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMyWithdrawalsQuery(cursor, pageSize),
            cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("withdrawals/{id:int}/cancel")]
    public async Task<IActionResult> CancelWithdrawal(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelWithdrawalCommand(id), cancellationToken);
        return this.ToActionResult(result);
    }
}
