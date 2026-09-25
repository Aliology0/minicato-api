using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Cases.Commands.StartCase;

public class StartCaseCommandValidator : AbstractValidator<StartCaseCommand>
{
    public StartCaseCommandValidator()
    {
        RuleFor(x => x.CaseId)
            .GreaterThan(0)
            .WithMessage(Messages.Generic.InvalidId(Messages.Fields.CaseId));
    }
}
