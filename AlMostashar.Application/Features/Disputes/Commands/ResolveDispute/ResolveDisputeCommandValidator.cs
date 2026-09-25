using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Disputes.Commands.ResolveDispute;

public class ResolveDisputeCommandValidator : AbstractValidator<ResolveDisputeCommand>
{
    public ResolveDisputeCommandValidator()
    {
        RuleFor(v => v.DisputeId)
            .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("DisputeId"));

        RuleFor(v => v.ResolutionType)
            .IsInEnum().WithMessage(Messages.Generic.InvalidEnumValue("ResolutionType"));

        RuleFor(v => v.AdminDecision)
            .NotEmpty().WithMessage(Messages.Generic.Required("AdminDecision"))
            .MaximumLength(2000).WithMessage(Messages.Generic.MaxLength("AdminDecision", 2000));

        RuleFor(v => v.AdminNotes)
            .NotEmpty().WithMessage(Messages.Generic.Required("AdminNotes"))
            .MaximumLength(4000).WithMessage(Messages.Generic.MaxLength("AdminNotes", 4000));
    }
}
