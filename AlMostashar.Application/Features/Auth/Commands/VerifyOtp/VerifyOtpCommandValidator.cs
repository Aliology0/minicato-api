using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Auth.Commands.VerifyOtp
{
    public class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
    {
        public VerifyOtpCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(Messages.Validation.EmailRequired)
                .EmailAddress().WithMessage(Messages.Validation.EmailInvalid);

            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage(Messages.Validation.OtpRequired)
                .Length(6).WithMessage(Messages.Validation.OtpLength);
        }
    }
}
