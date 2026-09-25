using AlMostashar.Api.DTOs;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Chat.DTOs;
using AlMostashar.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Api.SignalR
{
    [Authorize]
    public class AlMostasharHub: Hub
    {
        private readonly IAppDbContext _dbContext;
        private readonly ILogger<AlMostasharHub> logger;
        private readonly IConnectionTracker _connectionTracker;


        public AlMostasharHub(IAppDbContext dbContext, ILogger<AlMostasharHub> logger, IConnectionTracker connectionTracker)
        {
            _dbContext = dbContext;
            this.logger = logger;
            _connectionTracker = connectionTracker;
        }
        private int GetUserId()
        {
            int.TryParse(Context.UserIdentifier, out int Id);
            if (Id == 0) throw new HubException("User is not authenticated.");
            return Id;
        }
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            int userId = GetUserId();

            if (!_connectionTracker.IsUserActive(userId))
                await _dbContext.Users.Where(u => u.Id == userId).ExecuteUpdateAsync(setter => setter.SetProperty(x => x.IsActive, true));

            _connectionTracker.AddConnection(userId, Context.ConnectionId);
        }

        public async Task<ChatMessagesDto> SendMessage(SendMessageRequest messageRequest)
        {
            if (messageRequest.ChatId <= 0)
                throw new HubException("ChatId is required.");
            if (messageRequest.ReceiverId <= 0)
                throw new HubException("ReceiverId is required.");

            CancellationToken cancellationToken = Context.ConnectionAborted;
            var senderId = GetUserId();

            var chat = await _dbContext.Chats.Include(c=>c.ChatParticipants).FirstOrDefaultAsync(c=>c.Id == messageRequest.ChatId, cancellationToken);

            if(chat == null)
            {
                logger.LogWarning("Attempt to access unauthorized chat from userId: {senderId} chatId: {chatId}",
                    senderId, messageRequest.ChatId);
                throw new HubException("Chat not found.");
            }

            bool validSender = chat.ChatParticipants.Any(cp => cp.UserId == senderId);
            bool validReceiver = chat.ChatParticipants.Any(cp => cp.UserId == messageRequest.ReceiverId);

            if (!validSender || !validReceiver)
            {
                logger.LogWarning("Attempt to send unauthorized message from userId: {senderId} to {receiverId} from chatId: {chatId}",
                    senderId, messageRequest.ReceiverId, messageRequest.ChatId);
                throw new HubException("You are not authorized to send messages in this chat.");
            }

            CaseDocuments? document = null;
            if (messageRequest.DocumentId.HasValue)
            {
                document = await _dbContext.CaseDocuments.FirstOrDefaultAsync(d =>
                    d.Id == messageRequest.DocumentId.Value && d.UploadedByUserId == senderId &&
                    d.CaseId == null && d.ClientRequestId == null && d.ReportId == null && d.CleanupClaimToken == null, cancellationToken);
                if (document is null) throw new HubException("Document is missing, owned by another user, or already linked.");
                document.CaseId = chat.CaseId;
            }

            var msg = ChatMessage.Create(
                messageRequest.ChatId,
                senderId,
                messageRequest.ReceiverId,
                messageRequest.Content,
                messageRequest.MessageType,
                document);

            _dbContext.ChatMessages.Add(msg);
            chat.updateLastMessage(senderId, msg.Content, msg.MessageType, msg.SentAt);
            if (!msg.IsRead)
                chat.ChatParticipants.First(cp => cp.UserId == messageRequest.ReceiverId).UnReadMessageCount++;
            await _dbContext.SaveChangesAsync(cancellationToken);

            var messageDto = new ChatMessagesDto(msg.Id,
                senderId, messageRequest.MessageType,
                messageRequest.Content,
                false, msg.SentAt,
                null,
                document?.Id);

            await Clients.User(messageRequest.ReceiverId.ToString()).SendAsync("ReceiveMessage", messageRequest.ChatId, messageDto);

            return messageDto;
        }

        public async Task SendTypingIndicator(TypingIndicatorRequest request)
        {
            var senderId = GetUserId();

            await Clients.User(request.ReceiverId.ToString())
                         .SendAsync("ReceiveTypingIndicator", request.ChatId, senderId, request.IsTyping);
        }


        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);

            int userId = GetUserId();

            _connectionTracker.RemoveConnection(userId, Context.ConnectionId);

            if(!_connectionTracker.IsUserActive(userId))
                await _dbContext.Users.Where(u => u.Id == userId).ExecuteUpdateAsync(setter => setter.SetProperty(x => x.IsActive, false));

        }
    }
}
