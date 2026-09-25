using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Admin.Commands.RegisterAdmin
{
    public class RegisterAdminCommandValidator : AbstractValidator<RegisterAdminCommand>
    {
        public RegisterAdminCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage(Messages.Validation.FirstNameRequired)
                .MaximumLength(50).WithMessage(Messages.Validation.FirstNameMaxLength);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage(Messages.Validation.LastNameRequired)
                .MaximumLength(50).WithMessage(Messages.Validation.LastNameMaxLength);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(Messages.Validation.EmailRequired)
                .EmailAddress().WithMessage(Messages.Validation.EmailInvalid);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(Messages.Validation.PasswordRequired)
                .MinimumLength(8).WithMessage(Messages.Validation.PasswordMinLength)
                .Matches(@"[A-Z]").WithMessage(Messages.Validation.PasswordUppercase)
                .Matches(@"[a-z]").WithMessage(Messages.Validation.PasswordLowercase)
                .Matches(@"[0-9]").WithMessage(Messages.Validation.PasswordDigit);

            RuleFor(x => x.PhoneNo)
                .NotEmpty().WithMessage(Messages.Validation.PhoneRequired)
                .Matches(@"^\+?[0-9]{7,15}$").WithMessage(Messages.Validation.PhoneInvalid);

        }
    }
}
