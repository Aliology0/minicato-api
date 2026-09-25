using AlMostashar.Domain.Entities.Common;
using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Domain.Events;

/// <summary>
/// Raised after a new ChatMessage is sent. Used to trigger real-time notifications,
/// database notification records, and potentially third-party services like FCM in the future.
/// </summary>
public sealed class ChatMessageCreatedEvent : BaseEvent
{
    public int ChatId { get; }
    public int SenderId { get; }
    public int ReceiverId { get; }
    public string Content { get; }
    public MessageType MessageType { get; }

    public ChatMessageCreatedEvent(int chatId, int senderId, int receiverId, string content, MessageType messageType)
    {
        ChatId = chatId;
        SenderId = senderId;
        ReceiverId = receiverId;
        Content = content;
        MessageType = messageType;
    }
}
