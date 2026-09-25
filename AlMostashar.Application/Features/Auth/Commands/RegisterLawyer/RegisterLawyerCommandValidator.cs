using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Auth.Commands.RegisterLawyer
{
    public class RegisterLawyerCommandValidator : AbstractValidator<RegisterLawyerCommand>
    {
        public RegisterLawyerCommandValidator()
        {
            // ── Common User fields ────────────────────────────────────────────

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

            // ── Lawyer-specific fields ────────────────────────────────────────

            RuleFor(x => x.PhoneNo)
                .NotEmpty().WithMessage(Messages.Validation.PhoneRequired)
                .Matches(@"^\+?[0-9]{7,15}$").WithMessage(Messages.Validation.PhoneInvalid);

            RuleFor(x => x.SyndicateId)
                .GreaterThan(0).WithMessage(Messages.Validation.SyndicateIdRequired);

            RuleFor(x => x.AvatarUrl)
                .NotEmpty().WithMessage(Messages.Validation.ProfilePictureRequired)
                .MaximumLength(500).WithMessage(Messages.Generic.MaxLength("AvatarUrl", 500));

            RuleFor(x => x.SSN_Url)
                .NotEmpty().WithMessage(Messages.Validation.SsnRequired)
                .MaximumLength(500).WithMessage(Messages.Generic.MaxLength("SSN_Url", 500));

            RuleFor(x => x.SyndicateCardUrl)
                .NotEmpty().WithMessage(Messages.Validation.SyndicateCardRequired)
                .MaximumLength(500).WithMessage(Messages.Generic.MaxLength("SyndicateCardUrl", 500));

            RuleFor(x => x.GovernorateId)
                .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("GovernorateId"));

            RuleFor(x => x.CityId)
                .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("CityId"));
        }
    }
}
