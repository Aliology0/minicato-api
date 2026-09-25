using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlMostashar.Application.Features.Chat.EventHandlers
{
    public class CreateChatOnCaseCreatedHandler : INotificationHandler<CaseCreatedEvent>
    {
        private readonly IAppDbContext _appDbContext;
        private readonly IPublisher _publisher;
        private readonly ILogger<CreateChatOnCaseCreatedHandler> _logger;

        public CreateChatOnCaseCreatedHandler(IAppDbContext appDbContext, IPublisher publisher, ILogger<CreateChatOnCaseCreatedHandler> logger)
        {
            _appDbContext = appDbContext;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task Handle(CaseCreatedEvent notification, CancellationToken cancellationToken)
        {
            // 1. Check if chat already exists
            bool doesChatExist = await _appDbContext.Chats
                .AnyAsync(c => c.CaseId == notification.CaseId, cancellationToken);

            if (doesChatExist)
            {
                _logger.LogWarning($"Chat already exists due to a duplicate event. CaseId: {notification.CaseId}");
                return;
            }
            // 2. Verify if the case is valid for the given lawyer and client
            bool isValidCase = await _appDbContext.Cases
                .AnyAsync(x => x.Id == notification.CaseId &&
                x.LawyerId == notification.LawyerId &&
                x.CaseClientRequest!.ClientRequest.ClientId == notification.ClientId,
                cancellationToken);

            if (!isValidCase)
            {
                _logger.LogWarning($"Cannot create chat. Case is not valid for CaseId: {notification.CaseId}");
                return;
            }
            // 3. Create, save, and publish the new chat
            var chat = Domain.Entities.Chat.Create(notification.CaseId, notification.LawyerId, notification.ClientId);

            await _appDbContext.Chats.AddAsync(chat, cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            await _publisher.Publish(new ChatCreatedEvent(chat.Id, notification.LawyerId, notification.ClientId), cancellationToken);
        }
    }
}
