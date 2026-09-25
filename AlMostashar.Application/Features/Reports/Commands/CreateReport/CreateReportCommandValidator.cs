using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Reports.Commands.CreateReport;

public class CreateReportCommandValidator : AbstractValidator<CreateReportCommand>
{
    public CreateReportCommandValidator()
    {
        // ProblemContent is always required
        RuleFor(v => v.ProblemContent)
            .NotEmpty().WithMessage(Messages.Generic.Required("ProblemContent"))
            .MaximumLength(4000).WithMessage(Messages.Generic.MaxLength("ProblemContent", 4000));

        // LawyerId, when provided, must be positive
        RuleFor(v => v.LawyerId)
            .GreaterThan(0)
            .When(v => v.LawyerId.HasValue)
            .WithMessage(Messages.Generic.InvalidId("LawyerId"));

        // UserId, when provided, must be positive
        RuleFor(v => v.UserId)
            .GreaterThan(0)
            .When(v => v.UserId.HasValue)
            .WithMessage(Messages.Generic.InvalidId("UserId"));

        // CaseId, when provided, must be positive
        RuleFor(v => v.CaseId)
            .GreaterThan(0)
            .When(v => v.CaseId.HasValue)
            .WithMessage(Messages.Generic.InvalidId("CaseId"));

        // LawyerId and UserId cannot both be provided at the same time
        RuleFor(v => v)
            .Must(v => !(v.LawyerId.HasValue && v.UserId.HasValue))
            .WithMessage("Provide either LawyerId or UserId, not both.");

        // Reason must be a valid enum value
        RuleFor(v => v.Reason)
            .IsInEnum().WithMessage(Messages.Generic.InvalidEnumValue("Reason"));

        // Validate each submitted message evidence item
        RuleForEach(v => v.Messages)
            .ChildRules(msg =>
            {
                msg.RuleFor(m => m.SenderName)
                    .NotEmpty().WithMessage(Messages.Generic.Required("SenderName"))
                    .MaximumLength(200).WithMessage(Messages.Generic.MaxLength("SenderName", 200));

                msg.RuleFor(m => m.Content)
                    .NotEmpty().WithMessage(Messages.Generic.Required("Content"))
                    .MaximumLength(4000).WithMessage(Messages.Generic.MaxLength("Content", 4000));

                msg.RuleFor(m => m.SentAt)
                    .LessThanOrEqualTo(DateTime.UtcNow)
                    .WithMessage("'SentAt' must not be a future timestamp.");
            })
            .When(v => v.Messages != null && v.Messages.Count > 0);

        RuleFor(v => v.AttachmentIds)
            .Must(ids => ids == null || ids.Distinct().Count() <= 5)
            .WithMessage("Maximum of 5 attachments allowed.");

        RuleForEach(v => v.AttachmentIds)
            .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("AttachmentId"))
            .When(v => v.AttachmentIds != null && v.AttachmentIds.Count > 0);
    }
}
