using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Cases.Commands.FinishCase;

public class FinishCaseCommandValidator : AbstractValidator<FinishCaseCommand>
{
    public FinishCaseCommandValidator()
    {
        RuleFor(x => x.CaseId)
            .GreaterThan(0)
            .WithMessage(Messages.Generic.InvalidId(Messages.Fields.CaseId));
    }
}
