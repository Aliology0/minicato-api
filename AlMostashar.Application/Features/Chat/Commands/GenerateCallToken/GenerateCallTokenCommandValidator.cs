using AlMostashar.Application.Common.Constants;
using AlMostashar.Domain.ValueObject.Enum;
using FluentValidation;

namespace AlMostashar.Application.Features.Chat.Commands.GenerateCallToken
{
    public class GenerateCallTokenCommandValidator : AbstractValidator<GenerateCallTokenCommand>
    {
        public GenerateCallTokenCommandValidator()
        {
            RuleFor(x => x.ChatId)
                .GreaterThan(0).WithMessage(Messages.Generic.InvalidId("ChatId"))
                .NotNull().WithMessage(Messages.Generic.Required("ChatId"));

            RuleFor(x => x.CallType)
                .IsInEnum().WithMessage(Messages.Generic.InvalidEnumValue("CallType"));
        }
    }
}
