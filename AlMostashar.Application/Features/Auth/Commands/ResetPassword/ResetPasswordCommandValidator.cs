using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(x => x.ResetToken)
                .NotEmpty().WithMessage(Messages.Validation.ResetTokenRequired);

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage(Messages.Validation.NewPasswordRequired)
                .MinimumLength(6).WithMessage(Messages.Validation.PasswordMinLength);
        }
    }
}
