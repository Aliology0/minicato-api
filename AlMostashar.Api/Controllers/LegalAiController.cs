using AlMostashar.Application.Common.Exceptions;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.LegalAi.Commands.AskLegalAi;
using AlMostashar.Application.Features.LegalAi.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers;

[ApiController]
[Route("api/legal-ai")]
[Authorize]
public sealed class LegalAiController : ControllerBase
{
    private const string ServiceUnavailableMessage = "Legal AI service is temporarily unavailable.";

    private readonly IMediator _mediator;
    private readonly ILegalAiClient _legalAiClient;

    public LegalAiController(IMediator mediator, ILegalAiClient legalAiClient)
    {
        _mediator = mediator;
        _legalAiClient = legalAiClient;
    }

    /// <summary>
    /// Sends a legal question to the Egyptian Legal RAG engine.
    /// </summary>
    [HttpPost("chat")]
    [ProducesResponseType(typeof(LegalAiChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Chat(
        [FromBody] LegalAiChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AskLegalAiCommand(request.Query), cancellationToken);

        if (result.IsSuccess)
        {
            Response.Headers["X-Legal-AI-Cache"] = result.Value!.CacheHit ? "HIT" : "MISS";
            Response.Headers["X-Legal-AI-Elapsed-Ms"] = result.Value.ElapsedMilliseconds.ToString();
            return Ok(result.Value);
        }

        if (result.Error?.Code == "LegalAi.Unavailable")
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                error = ServiceUnavailableMessage
            });
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Returns sanitized Legal AI worker metadata for authenticated clients.
    /// </summary>
    [HttpGet("info")]
    [ProducesResponseType(typeof(LegalAiInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Info(CancellationToken cancellationToken)
    {
        try
        {
            var info = await _legalAiClient.GetInfoAsync(cancellationToken);
            return Ok(info);
        }
        catch (LegalAiServiceUnavailableException)
        {
            return ServiceUnavailable();
        }
    }

    /// <summary>
    /// Returns Legal AI worker health for authenticated admin/debug clients.
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(typeof(LegalAiHealthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Health(CancellationToken cancellationToken)
    {
        try
        {
            var health = await _legalAiClient.HealthAsync(cancellationToken);
            return Ok(health);
        }
        catch (LegalAiServiceUnavailableException)
        {
            return ServiceUnavailable();
        }
    }

    /// <summary>
    /// Warms the Legal AI worker. Intended for administrators or deployment automation only.
    /// </summary>
    [HttpPost("warmup")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Warmup(CancellationToken cancellationToken)
    {
        try
        {
            await _legalAiClient.WarmupAsync(cancellationToken);
            return Ok(new
            {
                status = "ok",
                message = "Legal AI warmup completed."
            });
        }
        catch (LegalAiServiceUnavailableException)
        {
            return ServiceUnavailable();
        }
    }

    private IActionResult ServiceUnavailable()
    {
        return StatusCode(StatusCodes.Status503ServiceUnavailable, new
        {
            error = ServiceUnavailableMessage
        });
    }
}
