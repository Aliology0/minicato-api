using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Chat.Commands.RejectCall
{
    public class RejectCallCommand : IRequest<Result<string>>
    {
        public int CallSessionId { get; set; }
    }
}
