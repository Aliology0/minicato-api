using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Cases.Commands.CancelCase;

public class CancelCaseCommandValidator : AbstractValidator<CancelCaseCommand>
{
    public CancelCaseCommandValidator()
    {
        RuleFor(x => x.CaseId)
            .GreaterThan(0)
            .WithMessage(Messages.Generic.InvalidId(Messages.Fields.CaseId));

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Cancellation reason is required when canceling a case.")
            .MaximumLength(1000)
            .WithMessage("Cancellation reason must not exceed 1000 characters.");
    }
}
