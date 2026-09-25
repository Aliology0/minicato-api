using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.ClientRequests.Commands.AcceptOffer;

public class AcceptOfferCommandValidator : AbstractValidator<AcceptOfferCommand>
{
    public AcceptOfferCommandValidator()
    {
        RuleFor(x => x.OfferId)
            .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("OfferId"));
    }
}
