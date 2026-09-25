using AlMostashar.Api.Helpers;
using AlMostashar.Application.Features.Payments.Commands.WebHook;
using AlMostashar.Application.Features.Payments.Commands.SimulateInvoicePaid;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AlMostashar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpPost("paymob-webhook")]
        public async Task<IActionResult> ReceiveWebHook([FromQuery] string hmac, [FromBody] JsonElement payload)
        {
            string fullGatewayResponse;
            try
            {
                fullGatewayResponse = payload.GetRawText();
            }
            catch (Exception)
            {
                return BadRequest(new { error = "Invalid JSON payload." });
            }

            PaymobWebHookDto? payloadObject;
            try
            {
                payloadObject = payload.Deserialize<PaymobWebHookDto>();
            }
            catch (JsonException)
            {
                return BadRequest(new { error = "Failed to deserialize webhook payload." });
            }

            if (payloadObject?.obj == null)
            {
                return BadRequest(new { error = "Webhook payload is missing required 'obj' field." });
            }

            var webHook = new WebHookCommand
            {
                Hmac = hmac ?? string.Empty,
                Payload = payloadObject,
                GatewayResponse = fullGatewayResponse
            };
            var result = await _mediator.Send(webHook);
            return this.ToActionResult(result);
        }

        [AllowAnonymous]
        [HttpGet("finished")]
        public IActionResult PaymentFinished()
        {
            return Ok();
        }

        [HttpPost("development/invoices/{invoiceId:int}/simulate-paid")]
        public async Task<IActionResult> SimulatePaid(
            int invoiceId,
            [FromServices] IWebHostEnvironment environment,
            CancellationToken cancellationToken)
        {
            if (!environment.IsDevelopment())
                return NotFound();
            var result = await _mediator.Send(
                new SimulateInvoicePaidCommand(invoiceId),
                cancellationToken);
            return this.ToActionResult(result);
        }
    }
}
