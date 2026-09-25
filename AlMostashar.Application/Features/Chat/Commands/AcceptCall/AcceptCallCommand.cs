using AlMostashar.Application.Common.Models;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Chat.Commands.AcceptCall
{
    public class AcceptCallCommand : IRequest<Result<IncomingCallDto>>
    {
        public int CallSessionId { get; set; }
    }
}
