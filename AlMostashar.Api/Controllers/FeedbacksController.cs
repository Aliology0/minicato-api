using AlMostashar.Api.Helpers;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Feedbacks.Commands.CreateFeedback;
using AlMostashar.Application.Features.Feedbacks.Queries.GetFeedbacks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers;

[ApiController]
[Route("api/client/feedbacks")]
[Authorize]
public class FeedbacksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public FeedbacksController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>Send feedback for a specific service received from a lawyer.</summary>
    [HttpPost]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> CreateFeedback(
        [FromBody] CreateFeedbackCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>Get received feedbacks via Cursor Pagination.</summary>
    [HttpGet]
    [Authorize(Roles = "Lawyer,Client")]
    public async Task<IActionResult> GetFeedbacks(
        [FromQuery] int? lawyerId, 
        [FromQuery] int? serviceId, 
        [FromQuery] int? nextCursor, 
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFeedbacksQuery
        {
            LawyerId = lawyerId,
            ServiceId = serviceId,
            NextCursor = nextCursor,
            PageSize = pageSize,
            IsLawyerRole = User.IsInRole("Lawyer"),
            CurrentUserId = _currentUserService.UserId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return this.ToActionResult(result);
    }
}
