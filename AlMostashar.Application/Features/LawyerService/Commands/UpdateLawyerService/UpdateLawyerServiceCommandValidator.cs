using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.LawyerService.Commands.UpdateLawyerService
{
    public class UpdateLawyerServiceCommandValidator : AbstractValidator<UpdateLawyerServiceCommand>
    {
        public UpdateLawyerServiceCommandValidator()
        {
            RuleFor(x => x.ServiceId)
                .GreaterThan(0)
                .WithMessage(Messages.Generic.InvalidId(Messages.Fields.ServiceId));

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Price.HasValue)
                .WithMessage(Messages.Generic.Negative(Messages.Fields.Price));

            RuleFor(x => x.Duration)
                .NotEmpty()
                .When(x => x.Duration != null)
                .WithMessage(Messages.Generic.Required(Messages.Fields.Duration));
        }
    }
}
