using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.Chat.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlMostashar.Application.Features.Chat.Queries.GetChatMessages
{
    public class GetChatMessagesQuery: IRequest<Result<CursorPagedResult<ChatMessagesDto>>>
    {
        public int ChatId { get; set; }
        public int? NextCursor { get; set; }
        public int PageSize { get; set; } = 20;
    }
}
