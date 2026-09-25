using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.Feedbacks.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Feedbacks.Queries.GetFeedbacks;

public class GetFeedbacksQuery : IRequest<Result<CursorPagedResult<FeedbackDto>>>
{
    public int? LawyerId { get; set; }
    public int? ServiceId { get; set; }
    public int? NextCursor { get; set; }
    public int PageSize { get; set; } = 10;

    // Populated from the Controller based on JWT
    public bool IsLawyerRole { get; set; }
    public int CurrentUserId { get; set; }
}
