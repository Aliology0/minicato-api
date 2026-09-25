using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Lookups.Queries.GetCities;

public class GetCitiesQueryValidator : AbstractValidator<GetCitiesQuery>
{
    public GetCitiesQueryValidator()
    {
        RuleFor(x => x.GovernorateId)
            .GreaterThan(0)
            .WithMessage(Messages.Generic.InvalidId("GovernorateId"));
    }
}
