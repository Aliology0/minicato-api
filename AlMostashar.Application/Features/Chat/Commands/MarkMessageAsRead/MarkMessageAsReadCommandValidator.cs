using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Chat.Commands.MarkMessageAsRead
{
    public class MarkMessageAsReadCommandValidator : AbstractValidator<MarkMessageAsReadCommand>
    {
        public MarkMessageAsReadCommandValidator()
        {
            RuleFor(x => x.ChatId)
            .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("ChatId"))
            .NotNull().WithMessage(Messages.Generic.Required("ChatId"));

            RuleFor(x => x.LastMessageId)
            .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("LastMessageId"))
            .When(x => x.LastMessageId.HasValue);
        }
    }
}
