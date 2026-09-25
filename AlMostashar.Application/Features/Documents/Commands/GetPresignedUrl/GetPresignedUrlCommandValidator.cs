using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Documents.Commands.GetPresignedUrl
{
    public class GetPresignedUrlCommandValidator : AbstractValidator<GetPresignedUrlCommand>
    {
        public GetPresignedUrlCommandValidator()
        {
            RuleFor(x => x.DocumentId)
                .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("DocumentId"));
            RuleFor(x => x.ExpirationMinutes)
                .InclusiveBetween(1, 1440)
                .When(x => x.ExpirationMinutes.HasValue);
        }
    }
}
