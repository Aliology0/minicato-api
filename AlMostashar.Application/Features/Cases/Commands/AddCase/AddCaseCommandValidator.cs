using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Cases.Commands.AddCase;

public class AddCaseCommandValidator : AbstractValidator<AddCaseCommand>
{
    public AddCaseCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage(Messages.Generic.Required(Messages.Fields.CaseTitle))
            .MaximumLength(200)
            .WithMessage(Messages.Generic.MaxLength(Messages.Fields.CaseTitle, 200));

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage(Messages.Generic.Required(Messages.Fields.CaseDescription))
            .MaximumLength(2000)
            .WithMessage(Messages.Generic.MaxLength(Messages.Fields.CaseDescription, 2000));

        RuleFor(x => x.LawyerId)
            .GreaterThan(0)
            .When(x => x.LawyerId.HasValue);

        RuleFor(x => x.ClientRequestId)
            .GreaterThan(0)
            .When(x => x.ClientRequestId.HasValue);
    }
}
