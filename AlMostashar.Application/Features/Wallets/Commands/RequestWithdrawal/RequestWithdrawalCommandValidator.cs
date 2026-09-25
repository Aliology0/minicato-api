using FluentValidation;

namespace AlMostashar.Application.Features.Wallets.Commands.RequestWithdrawal;

public class RequestWithdrawalCommandValidator : AbstractValidator<RequestWithdrawalCommand>
{
    public RequestWithdrawalCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Method)
            .IsInEnum();

        RuleFor(x => x.AccountDetails)
            .NotEmpty()
            .MaximumLength(1000);
    }
}
