using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Auth.Commands.RegisterClient
{
    public class RegisterClientCommandValidator : AbstractValidator<RegisterClientCommand>
    {
        public RegisterClientCommandValidator()
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
                .MinimumLength(6).WithMessage(Messages.Validation.PasswordMinLength)
                .Matches(@"[A-Z]").WithMessage(Messages.Validation.PasswordUppercase)
                .Matches(@"[a-z]").WithMessage(Messages.Validation.PasswordLowercase)
                .Matches(@"[0-9]").WithMessage(Messages.Validation.PasswordDigit);

            RuleFor(x => x.AvatarUrl)
                .NotEmpty().WithMessage(Messages.Validation.ProfilePictureRequired)
                .MaximumLength(500).WithMessage(Messages.Generic.MaxLength("AvatarUrl", 500));

            RuleFor(x => x.NationalIdPhotoUrl)
                .NotEmpty().WithMessage(Messages.Validation.NationalIdPhotoRequired)
                .MaximumLength(500).WithMessage(Messages.Generic.MaxLength("NationalIdPhotoUrl", 500));
        }
    }
}
