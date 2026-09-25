using AlMostashar.Application.Common.Constants;
using AlMostashar.Domain.ValueObject.Enum;
using FluentValidation;

namespace AlMostashar.Application.Features.LegalService.Commands.AddLegalService
{
    public class AddLegalServiceCommandValidator : AbstractValidator<AddLegalServiceCommand>
    {
        public AddLegalServiceCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage(Messages.Generic.Required(Messages.Fields.ServiceTitle))
                .MaximumLength(100)
                .WithMessage(Messages.Generic.MaxLength(Messages.Fields.ServiceTitle, 100));

            RuleFor(x => x.Summary)
                .NotEmpty()
                .WithMessage(Messages.Generic.Required(Messages.Fields.ServiceSummary))
                .MaximumLength(500)
                .WithMessage(Messages.Generic.MaxLength(Messages.Fields.ServiceSummary, 500));

            RuleFor(x => x.FullDescription)
                .NotEmpty()
                .WithMessage(Messages.Generic.Required(Messages.Fields.ServiceFullDescription));

            RuleFor(x => x.ServiceType)
                .Must(v => Enum.IsDefined(typeof(ServiceType), v))
                .WithMessage(Messages.Generic.InvalidEnumValue(Messages.Fields.ServiceType));

            RuleFor(x => x.ExpectedDuration)
                .MaximumLength(50)
                .When(x => x.ExpectedDuration is not null)
                .WithMessage(Messages.Generic.MaxLength(Messages.Fields.ExpectedDuration, 50));
        }
    }
}
