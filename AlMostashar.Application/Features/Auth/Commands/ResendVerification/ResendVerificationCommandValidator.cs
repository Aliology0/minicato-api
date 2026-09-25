using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Auth.Commands.ResendVerification
{
    public class ResendVerificationCommandValidator : AbstractValidator<ResendVerificationCommand>
    {
        public ResendVerificationCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(Messages.Validation.EmailRequired)
                .EmailAddress().WithMessage(Messages.Validation.EmailInvalid);
        }
    }
}
