using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.ClientRequests.Commands.CreateDirectRequest;

public class CreateDirectRequestCommandValidator : AbstractValidator<CreateDirectRequestCommand>
{
    public CreateDirectRequestCommandValidator()
    {
        RuleFor(x => x.LawyerId)
            .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("LawyerId"));

        RuleFor(x => x.LegalServiceId)
            .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("LegalServiceId"));

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(Messages.Generic.Required("Title"))
            .MaximumLength(300).WithMessage(Messages.Generic.MaxLength("Title", 300));

        RuleFor(x => x.ProblemDetails)
            .NotEmpty().WithMessage(Messages.Generic.Required("ProblemDetails"))
            .MaximumLength(4000).WithMessage(Messages.Generic.MaxLength("ProblemDetails", 4000));

        RuleFor(x => x.GovernorateId)
            .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("GovernorateId"));

        RuleFor(x => x.CityId)
            .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("CityId"));

        RuleFor(x => x.ClientDeadline)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .When(x => x.ClientDeadline.HasValue);
        RuleFor(x => x.Urgency).IsInEnum();
        RuleFor(x => x.PreferredCommunicationMethod).IsInEnum().When(x => x.PreferredCommunicationMethod.HasValue);
        RuleFor(x => x.RequestDetails.ValueKind)
            .Must(kind => kind is not System.Text.Json.JsonValueKind.Undefined and not System.Text.Json.JsonValueKind.Null)
            .WithMessage("RequestDetails is required.");
        RuleForEach(x => x.AttachmentIds).GreaterThan(0);
    }
}
