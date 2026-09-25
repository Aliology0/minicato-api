using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Features.Chat.Queries.GetChatMessages;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlMostashar.Application.Features.Chat.Queries.GetChats
{
    internal class GetChatsQueryValidator : AbstractValidator<GetChatsQuery>
    {
        public GetChatsQueryValidator()
        {
            RuleFor(x => x.Search)
                .MaximumLength(50)
                .MinimumLength(1)
                .When(x=>x.Search != null)
                .WithMessage(Messages.Generic.InvalidId("Search"));

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 50)
                .WithMessage("PageSize must be between 1 and 50.");

            RuleFor(x => x.NextCursor)
                .GreaterThan(0)
                .When(x => x.NextCursor.HasValue)
                .WithMessage(Messages.Generic.InvalidId("NextCursor"));
        }
    }

}
