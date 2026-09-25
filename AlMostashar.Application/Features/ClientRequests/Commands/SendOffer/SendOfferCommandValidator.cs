using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.ClientRequests.Commands.SendOffer;

public class SendOfferCommandValidator : AbstractValidator<SendOfferCommand>
{
    public SendOfferCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("RequestId"));

        RuleFor(x => x.LegalServiceId)
            .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("LegalServiceId"))
            .When(x => x.LegalServiceId.HasValue);

        RuleFor(x => x.OfferedAmount)
            .GreaterThan(0).WithMessage(Messages.Generic.Negative("OfferedAmount"));

        RuleFor(x => x.Note)
            .MaximumLength(2000).WithMessage(Messages.Generic.MaxLength("Note", 2000))
            .When(x => x.Note is not null);
    }
}
