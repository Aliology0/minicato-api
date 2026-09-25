using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Feedbacks.Commands.CreateFeedback;

public class CreateFeedbackCommand : IRequest<Result<string>>
{
    public int ClientRequestId { get; set; }
    public int LawyerId { get; set; }
    public int ServiceId { get; set; }
    public double Rating { get; set; }
    public string? Content { get; set; }
}
