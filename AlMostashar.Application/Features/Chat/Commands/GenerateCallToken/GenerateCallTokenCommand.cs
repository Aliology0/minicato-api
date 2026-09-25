using AlMostashar.Application.Features.Chat.DTOs;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;

namespace AlMostashar.Application.Features.Chat.Commands.GenerateCallToken
{
    public class GenerateCallTokenCommand : IRequest<Result<CallTokenResponse>>
    {
        public int ChatId { get; set; }
        public CallType CallType { get; set; }
    }
}
