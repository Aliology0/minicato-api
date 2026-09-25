using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.ClientRequests.Queries.GetBroadcastRequests;

public class GetBroadcastRequestsQueryValidator : AbstractValidator<GetBroadcastRequestsQuery>
{
    public GetBroadcastRequestsQueryValidator()
    {
        RuleFor(x => x.GovernorateId)
            .GreaterThan(0)
            .When(x => x.GovernorateId.HasValue)
            .WithMessage(Messages.Generic.InvalidId("GovernorateId"));

        RuleFor(x => x.CityId)
            .GreaterThan(0)
            .When(x => x.CityId.HasValue)
            .WithMessage(Messages.Generic.InvalidId("CityId"));

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("PageSize must be between 1 and 50.");

        RuleFor(x => x)
            .Must(x => !x.MinBudget.HasValue || !x.MaxBudget.HasValue || x.MinBudget.Value <= x.MaxBudget.Value)
            .WithMessage("MinBudget must be less than or equal to MaxBudget.");
    }
}
