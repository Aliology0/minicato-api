using FluentValidation;

namespace AlMostashar.Application.Features.Wallets.Commands.MarkWithdrawalPaid;

public class MarkWithdrawalPaidCommandValidator : AbstractValidator<MarkWithdrawalPaidCommand>
{
    public MarkWithdrawalPaidCommandValidator()
    {
        RuleFor(x => x.PayoutReference)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(x => x.PayoutProvider)
            .MaximumLength(100);

        RuleFor(x => x.AdminNotes)
            .MaximumLength(1000);
    }
}
