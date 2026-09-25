using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.ClientProfile.Commands.ChangePassword;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(v => v.OldPassword)
            .NotEmpty().WithMessage(Messages.Validation.PasswordRequired);

        RuleFor(v => v.NewPassword)
            .NotEmpty().WithMessage(Messages.Validation.NewPasswordRequired)
            .MinimumLength(8).WithMessage(Messages.Validation.PasswordMinLength)
            .Matches("[A-Z]").WithMessage(Messages.Validation.PasswordUppercase)
            .Matches("[a-z]").WithMessage(Messages.Validation.PasswordLowercase)
            .Matches("[0-9]").WithMessage(Messages.Validation.PasswordDigit);
    }
}
