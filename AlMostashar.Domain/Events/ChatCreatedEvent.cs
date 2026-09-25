using MediatR;

namespace AlMostashar.Domain.Events
{
    public class ChatCreatedEvent: INotification
    {
        public int ChatId { get; init; }
        public int LawyerId { get; init; }
        public int ClientId { get; init; }

        public ChatCreatedEvent(int chatId, int lawyerId, int clientId)
        {
            ChatId = chatId;
            LawyerId = lawyerId;
            ClientId = clientId;
        }
    }
}
