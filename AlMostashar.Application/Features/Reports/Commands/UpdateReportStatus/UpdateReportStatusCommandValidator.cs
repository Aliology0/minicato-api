using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Reports.Commands.UpdateReportStatus;

public class UpdateReportStatusCommandValidator : AbstractValidator<UpdateReportStatusCommand>
{
    public UpdateReportStatusCommandValidator()
    {
        RuleFor(v => v.ReportId)
            .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("ReportId"));

        RuleFor(v => v.Status)
            .IsInEnum().WithMessage(Messages.Generic.InvalidEnumValue("Status"));

        RuleFor(v => v.AdminNotes)
            .MaximumLength(4000).WithMessage(Messages.Generic.MaxLength("AdminNotes", 4000));
    }
}
