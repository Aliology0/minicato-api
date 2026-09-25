using AlMostashar.Api.DTOs;
using AlMostashar.Api.Helpers;
using AlMostashar.Application.Features.Chat.Commands.AcceptCall;
using AlMostashar.Application.Features.Chat.Commands.EndCall;
using AlMostashar.Application.Features.Chat.Commands.GenerateCallToken;
using AlMostashar.Application.Features.Chat.Commands.MarkMessageAsRead;
using AlMostashar.Application.Features.Chat.Commands.RejectCall;
using AlMostashar.Application.Features.Chat.Queries.GetChatMessages;
using AlMostashar.Application.Features.Chat.Queries.GetChats;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMostashar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ChatsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetChats([FromQuery]GetChatsQuery chatsQuery,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(chatsQuery,cancellationToken);
            return this.ToActionResult(result);
        }


        [HttpGet("Messages")]
        public async Task<IActionResult> GetChatMessages([FromQuery] GetChatMessagesQuery chatMessagesQuery,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(chatMessagesQuery, cancellationToken);
            return this.ToActionResult(result);
        }


        [HttpPut("{Id:int}/messages/{LastReadMessageId:int}/mark-read")]
        public async Task<IActionResult> MarkMessageAsRead(int Id, int LastReadMessageId, CancellationToken cancellationToken)
        {
            MarkMessageAsReadCommand command = new MarkMessageAsReadCommand { ChatId = Id, LastMessageId = LastReadMessageId };
            var result= await _mediator.Send(command, cancellationToken); 
            return this.ToActionResult(result);
        }


        /// <summary>
        /// Generates an Agora call token for the given chat.
        /// If an active call already exists, returns the existing session's token (reconnection).
        /// </summary>
        [HttpPost("{chatId:int}/call")]
        public async Task<IActionResult> GenerateCallToken(int chatId, [FromBody] GenerateCallTokenRequest TokenRequest, CancellationToken cancellationToken)
        {
            var command = new GenerateCallTokenCommand { ChatId = chatId, CallType = TokenRequest.CallType};
            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>
        /// Accepts an incoming call. Updates the call status to Ongoing and notifies the caller.
        /// </summary>
        [HttpPut("calls/{Id:int}/accept")]
        public async Task<IActionResult> AcceptCall(int Id, CancellationToken cancellationToken)
        {
            var command = new AcceptCallCommand { CallSessionId= Id };
            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>
        /// Rejects an incoming call. Updates the call status to Rejected and notifies the caller.
        /// </summary>
        [HttpPut("calls/{Id:int}/reject")]
        public async Task<IActionResult> RejectCall(int Id, CancellationToken cancellationToken)
        {
            var command = new RejectCallCommand { CallSessionId = Id };
            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

        /// <summary>
        /// Ends an ongoing call. Updates the call status to Completed and notifies the other participant.
        /// </summary>
        [HttpPut("calls/{Id:int}/end")]
        public async Task<IActionResult> EndCall(int Id, CancellationToken cancellationToken)
        {
            var command = new EndCallCommand { CallSessionId = Id };
            var result = await _mediator.Send(command, cancellationToken);
            return this.ToActionResult(result);
        }

    }
}
