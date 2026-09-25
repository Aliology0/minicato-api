using AlMostashar.Application.Common.Constants;
using AlMostashar.Domain.ValueObject.Enum;
using FluentValidation;

namespace AlMostashar.Application.Features.Admin.Queries.GetUsers;

public class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        // PageSize: 1–50
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("PageSize must be between 1 and 50.");

        // Search: meaningful length when provided
        RuleFor(x => x.Search)
            .MinimumLength(2).WithMessage("Search term must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Search term must not exceed 100 characters.")
            .When(x => x.Search != null);

        // Cursor: must be positive when provided
        RuleFor(x => x.Cursor)
            .GreaterThan(0)
            .WithMessage(Messages.Generic.InvalidId("Cursor"))
            .When(x => x.Cursor.HasValue);

        // CursorDate: must be provided together with Cursor
        RuleFor(x => x.CursorDate)
            .NotNull()
            .WithMessage("CursorDate is required when Cursor is provided.")
            .When(x => x.Cursor.HasValue);

        RuleFor(x => x.Cursor)
            .NotNull()
            .WithMessage("Cursor is required when CursorDate is provided.")
            .When(x => x.CursorDate.HasValue);

        // IsVerified only makes sense when filtering by Lawyer role
        RuleFor(x => x.IsVerified)
            .Must((query, _) => query.Role == UserRole.Lawyer || query.Role == null)
            .WithMessage("IsVerified filter can only be used when Role is Lawyer or not set.")
            .When(x => x.IsVerified.HasValue);
    }
}
