using FluentValidation;

namespace AlMostashar.Application.Features.Wallets.Commands.RejectWithdrawal;

public class RejectWithdrawalCommandValidator : AbstractValidator<RejectWithdrawalCommand>
{
    public RejectWithdrawalCommandValidator()
    {
        RuleFor(x => x.RejectionReason)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.AdminNotes)
            .MaximumLength(1000);
    }
}
