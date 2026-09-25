using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Helpers;
using FluentValidation;

namespace AlMostashar.Application.Features.Disputes.Commands.OpenDispute;

public class OpenDisputeCommandValidator : AbstractValidator<OpenDisputeCommand>
{
    public OpenDisputeCommandValidator()
    {
        RuleFor(v => v.CaseId)
            .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("CaseId"));

        RuleFor(v => v.Reason)
            .NotEmpty().WithMessage(Messages.Generic.Required("Reason"))
            .MaximumLength(4000).WithMessage(Messages.Generic.MaxLength("Reason", 4000));

        RuleFor(v => v.Attachments)
            .Must(a => a == null || a.Count <= 10)
            .WithMessage("A maximum of 10 attachments are allowed.");

        RuleForEach(v => v.Attachments)
            .ChildRules(a =>
            {
                a.RuleFor(x => x.FileName)
                    .NotEmpty().WithMessage(Messages.Generic.Required("Attachment Name"))
                    .MaximumLength(500).WithMessage(Messages.Generic.MaxLength("Attachment Name", 500))
                    .Must(fileName => 
                    {
                        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
                        return !string.IsNullOrEmpty(ext) && FileContentTypeHelper.AllowedExtensions.Contains(ext);
                    })
                    .WithMessage($"Attachment must be one of: {string.Join(", ", FileContentTypeHelper.AllowedExtensions)}.");

                a.RuleFor(x => x.Length)
                    .GreaterThan(0).WithMessage("Attachment size must be greater than zero.")
                    .LessThanOrEqualTo(20 * 1024 * 1024).WithMessage("Attachment size must not exceed 20MB.");
            })
            .When(v => v.Attachments != null && v.Attachments.Count > 0);

        RuleForEach(v => v.DisputeChatMessages)
            .ChildRules(m =>
            {
                m.RuleFor(x => x.SenderId)
                    .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("SenderId"));

                m.RuleFor(x => x.Content)
                    .NotEmpty().WithMessage(Messages.Generic.Required("Message Content"))
                    .MaximumLength(4000).WithMessage(Messages.Generic.MaxLength("Message Content", 4000));
            })
            .When(v => v.DisputeChatMessages != null && v.DisputeChatMessages.Count > 0);
    }
}

