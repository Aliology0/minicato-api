using FluentValidation;
using AlMostashar.Application.Common.Constants;

namespace AlMostashar.Application.Features.Feedbacks.Commands.CreateFeedback;

public class CreateFeedbackCommandValidator : AbstractValidator<CreateFeedbackCommand>
{
    public CreateFeedbackCommandValidator()
    {
        RuleFor(v => v.ClientRequestId)
            .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("ClientRequestId"));

        RuleFor(v => v.LawyerId)
            .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("LawyerId"));

        RuleFor(v => v.ServiceId)
            .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("ServiceId"));

        RuleFor(v => v.Rating)
            .InclusiveBetween(1, 5).WithMessage(Messages.Validation.RatingMustBeBetween1And5);

        RuleFor(v => v.Content)
            .MaximumLength(1000).WithMessage(Messages.Generic.MaxLength(Messages.Fields.FeedbackContent, 1000));
    }
}
