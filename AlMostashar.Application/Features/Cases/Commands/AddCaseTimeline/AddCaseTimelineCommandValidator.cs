using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Cases.Commands.AddCaseTimeline;

public class AddCaseTimelineCommandValidator : AbstractValidator<AddCaseTimelineCommand>
{
    public AddCaseTimelineCommandValidator()
    {
        RuleFor(x => x.CaseId)
            .GreaterThan(0)
            .WithMessage(Messages.Generic.InvalidId(Messages.Fields.CaseId));

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage(Messages.Generic.Required(Messages.Fields.TimelineTitle))
            .MaximumLength(200)
            .WithMessage(Messages.Generic.MaxLength(Messages.Fields.TimelineTitle, 200));

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage(Messages.Generic.Required(Messages.Fields.TimelineContent))
            .MaximumLength(2000)
            .WithMessage(Messages.Generic.MaxLength(Messages.Fields.TimelineContent, 2000));
    }
}
