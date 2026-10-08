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

            RuleFor(x => x.GovernorateId)
                .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("GovernorateId"));

            RuleFor(x => x.CityId)
                .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("CityId"));

            RuleFor(x => x.YearsOfExperience)
                .GreaterThanOrEqualTo(0).WithMessage("Years of experience must be 0 or greater.")
                .LessThanOrEqualTo(70).WithMessage("Years of experience cannot exceed 70 years.");

            RuleFor(x => x.SpecializationIds)
                .NotNull().WithMessage("Specialization IDs are required.")
                .NotEmpty().WithMessage("At least one specialization must be selected.")
                .ForEach(rule => rule.GreaterThan(0).WithMessage("Each specialization ID must be greater than 0."));
        }
    }
}
