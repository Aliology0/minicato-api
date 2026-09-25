using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Cases.Commands.ConfirmCaseCompletion;

public class ConfirmCaseCompletionCommandValidator : AbstractValidator<ConfirmCaseCompletionCommand>
{
    public ConfirmCaseCompletionCommandValidator()
    {
        RuleFor(x => x.CaseId)
            .GreaterThan(0)
            .WithMessage(Messages.Generic.InvalidId(Messages.Fields.CaseId));
    }
}
