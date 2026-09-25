using FluentValidation;

namespace AlMostashar.Application.Features.Wallets.Commands.ApproveWithdrawal;

public class ApproveWithdrawalCommandValidator : AbstractValidator<ApproveWithdrawalCommand>
{
    public ApproveWithdrawalCommandValidator()
    {
        RuleFor(x => x.AdminNotes)
            .MaximumLength(1000);
    }
}
