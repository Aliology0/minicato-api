using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.ClientRequests.Queries.GetLawyersByService;

public class GetLawyersByServiceQueryValidator : AbstractValidator<GetLawyersByServiceQuery>
{
    public GetLawyersByServiceQueryValidator()
    {
        RuleFor(x => x.LegalServiceId)
            .GreaterThan(0)
            .WithMessage(Messages.Generic.InvalidId("LegalServiceId"));

        RuleFor(x => x.GovernorateId)
            .GreaterThan(0)
            .When(x => x.GovernorateId.HasValue)
            .WithMessage(Messages.Generic.InvalidId("GovernorateId"));

        RuleFor(x => x.CityId)
            .GreaterThan(0)
            .When(x => x.CityId.HasValue)
            .WithMessage(Messages.Generic.InvalidId("CityId"));
    }
}
