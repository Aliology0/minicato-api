using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Disputes.Queries.GetDisputesList;

public class GetDisputesListQueryValidator : AbstractValidator<GetDisputesListQuery>
{
    public GetDisputesListQueryValidator()
    {
        RuleFor(v => v.Status)
            .IsInEnum().WithMessage(Messages.Generic.InvalidEnumValue("Status"))
            .When(v => v.Status.HasValue);

        RuleFor(v => v.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
            .LessThanOrEqualTo(50).WithMessage("PageSize must be less than or equal to 50.");
    }
}
