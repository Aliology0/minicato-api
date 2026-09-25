using AlMostashar.Api.Helpers;
using AlMostashar.Application.Features.Payments.Commands.AdminRefundPayment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers;

[ApiController]
[Route("api/admin/payments")]
[Authorize(Roles = "Admin")]
public class AdminPaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminPaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Refund a payment directly (full or partial).
    /// If RefundedAmount is null, the full remaining refundable amount is refunded.
    /// </summary>
    [HttpPost("{paymentId:int}/refund")]
    public async Task<IActionResult> RefundPayment(
        int paymentId,
        [FromBody] AdminRefundPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new AdminRefundPaymentCommand(paymentId, request.RefundedAmount, request.Reason),
            cancellationToken);

        return this.ToActionResult(result);
    }
}

/// <summary>
/// Request body for admin direct refund.
/// RefundedAmount is in EGP. If null, full remaining refundable amount is refunded.
/// </summary>
public sealed class AdminRefundPaymentRequest
{
    public decimal? RefundedAmount { get; set; }
    public string? Reason { get; set; }
}
