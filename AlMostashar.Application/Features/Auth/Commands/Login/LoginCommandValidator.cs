using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Auth.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(Messages.Validation.EmailRequired)
                .EmailAddress().WithMessage(Messages.Validation.EmailInvalid);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(Messages.Validation.PasswordRequired)
                .MinimumLength(6).WithMessage(Messages.Validation.PasswordMinLength);
        }
    }
}
