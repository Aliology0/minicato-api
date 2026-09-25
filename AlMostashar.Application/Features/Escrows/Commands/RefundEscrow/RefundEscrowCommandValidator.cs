using FluentValidation;

namespace AlMostashar.Application.Features.Escrows.Commands.RefundEscrow;

public class RefundEscrowCommandValidator : AbstractValidator<RefundEscrowCommand>
{
    public RefundEscrowCommandValidator()
    {
        RuleFor(x => x.EscrowId)
            .GreaterThan(0);
    }
}
