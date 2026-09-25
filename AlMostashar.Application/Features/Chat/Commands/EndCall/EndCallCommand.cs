using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Chat.Commands.EndCall
{
    public class EndCallCommand : IRequest<Result<string>>
    {
        public int CallSessionId { get; set; }
    }
}
