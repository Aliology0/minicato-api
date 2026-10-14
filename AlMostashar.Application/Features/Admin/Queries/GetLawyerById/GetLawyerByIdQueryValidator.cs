using FluentValidation;

namespace AlMostashar.Application.Features.Admin.Queries.GetLawyerById;

public class GetLawyerByIdQueryValidator : AbstractValidator<GetLawyerByIdQuery>
{
    public GetLawyerByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id must be a positive integer.");
    }
}
