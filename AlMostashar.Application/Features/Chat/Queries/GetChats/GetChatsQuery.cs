using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.Chat.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Chat.Queries.GetChats
{
    public class GetChatsQuery: IRequest<Result<ChatResponse>>
    {
        public string? Search { get; set; }
        public int? NextCursor { get; set; }
        public DateTime? CursorDate { get; set; }
        public int PageSize { get; set; } = 10;
    }
}
