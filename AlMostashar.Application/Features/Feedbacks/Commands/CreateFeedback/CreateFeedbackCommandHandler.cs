using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Feedbacks.Commands.CreateFeedback;

public class CreateFeedbackCommandHandler : IRequestHandler<CreateFeedbackCommand, Result<string>>
{
    private readonly IAppDbContext _context;

    public CreateFeedbackCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(CreateFeedbackCommand request, CancellationToken cancellationToken)
    {
        // Check if the ClientRequest exists
        var clientRequestExists = await _context.ClientRequests
            .AnyAsync(cr => cr.Id == request.ClientRequestId, cancellationToken);

        if (!clientRequestExists)
        {
            return Result<string>.Failure(new Error("ClientRequest.NotFound", Messages.Generic.NotFound("ClientRequest")));
        }

        // Check if the LawyerService exists
        var lawyerServiceExists = await _context.LawyerServices
            .AnyAsync(ls => ls.LawyerId == request.LawyerId && ls.LegalServiceId == request.ServiceId, cancellationToken);

        if (!lawyerServiceExists)
        {
            return Result<string>.Failure(new Error("LawyerService.NotFound", Messages.Generic.NotFound("LawyerService")));
        }

        // Check if a feedback already exists for this request
        var feedbackExists = await _context.Feedbacks
            .AnyAsync(f => f.ClientRequestId == request.ClientRequestId, cancellationToken);

        if (feedbackExists)
        {
            return Result<string>.Failure(new Error("Feedback.AlreadyExists", Messages.Generic.AlreadyExists("Feedback")));
        }

        var feedback = new Feedback
        {
            Rate = request.Rating,
            Content = request.Content,
            ClientRequestId = request.ClientRequestId,
            LawyerServiceLawyerId = request.LawyerId,
            LawyerServiceLegalServiceId = request.ServiceId
        };

        _context.Feedbacks.Add(feedback);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(Messages.GeneralSuccess.FeedbackSentSuccessfully);
    }
}
