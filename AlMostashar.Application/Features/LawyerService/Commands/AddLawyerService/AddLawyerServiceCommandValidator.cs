using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.LawyerService.Commands.AddLawyerService
{
    public class AddLawyerServiceCommandValidator : AbstractValidator<AddLawyerServiceCommand>
    {
        public AddLawyerServiceCommandValidator()
        {
            RuleFor(x => x.ServiceId)
                .GreaterThan(0)
                .WithMessage(Messages.Generic.InvalidId(Messages.Fields.ServiceId));

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                .WithMessage(Messages.Generic.Negative(Messages.Fields.Price));

            RuleFor(x => x.Duration)
                .NotEmpty()
                .WithMessage(Messages.Generic.Required(Messages.Fields.Duration));
        }
    }
}
