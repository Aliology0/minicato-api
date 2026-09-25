using FluentValidation;

namespace AlMostashar.Application.Features.Escrows.Commands.MarkEscrowAsDisputed;

public class MarkEscrowAsDisputedCommandValidator : AbstractValidator<MarkEscrowAsDisputedCommand>
{
    public MarkEscrowAsDisputedCommandValidator()
    {
        RuleFor(x => x.EscrowId)
            .GreaterThan(0);
    }
}
