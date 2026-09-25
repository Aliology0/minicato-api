using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.ClientRequests.Commands.RejectOffer;

public class RejectOfferCommandValidator : AbstractValidator<RejectOfferCommand>
{
    public RejectOfferCommandValidator()
    {
        RuleFor(x => x.OfferId)
            .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("OfferId"));
    }
}
