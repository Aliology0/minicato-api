using FluentValidation;

namespace AlMostashar.Application.Features.LegalAi.Commands.AskLegalAi;

public sealed class AskLegalAiCommandValidator : AbstractValidator<AskLegalAiCommand>
{
    public AskLegalAiCommandValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("'Query' must not be empty.")
            .MaximumLength(2000).WithMessage("'Query' must not exceed 2000 characters.");
    }
}
