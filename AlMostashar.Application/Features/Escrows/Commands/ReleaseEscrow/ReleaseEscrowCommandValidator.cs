using FluentValidation;

namespace AlMostashar.Application.Features.Escrows.Commands.ReleaseEscrow;

public class ReleaseEscrowCommandValidator : AbstractValidator<ReleaseEscrowCommand>
{
    public ReleaseEscrowCommandValidator()
    {
        RuleFor(x => x.EscrowId)
            .GreaterThan(0);
    }
}
