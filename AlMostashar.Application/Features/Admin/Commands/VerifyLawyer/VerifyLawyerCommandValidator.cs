using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Admin.Commands.VerifyLawyer
{
    public class VerifyLawyerCommandValidator : AbstractValidator<VerifyLawyerCommand>
    {
        public VerifyLawyerCommandValidator()
        {
            RuleFor(x => x.LawyerId)
                .GreaterThan(0).WithMessage(Messages.Admin.LawyerNotFound);
        }
    }
}
