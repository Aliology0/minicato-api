using AlMostashar.Application.Common.Constants;
using FluentValidation;

namespace AlMostashar.Application.Features.Chat.Queries.GetChatMessages;

public class GetChatMessagesQueryValidator : AbstractValidator<GetChatMessagesQuery>
{
    public GetChatMessagesQueryValidator()
    {
        RuleFor(x => x.ChatId)
            .GreaterThan(0)
            .WithMessage(Messages.Generic.InvalidId("ChatId"));

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("PageSize must be between 1 and 50.");

        RuleFor(x => x.NextCursor)
            .GreaterThan(0)
            .When(x => x.NextCursor.HasValue)
            .WithMessage(Messages.Generic.InvalidId("NextCursor"));
    }
}
