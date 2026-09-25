using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Auth.Commands.VerifyEmail
{
    public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
    {
        public VerifyEmailCommandValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage(Messages.Validation.UserIdRequired);

            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage(Messages.Validation.OtpRequired)
                .Length(6).WithMessage(Messages.Validation.OtpLength);
        }
    }
}
