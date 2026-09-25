using AlMostashar.Domain.Events;
using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public int SenderId { get; private set; }
    public string Content { get; private set; } = null!;
    public DateTime SentAt { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public MessageType MessageType { get; private set; }

    public int? CaseDocumentId { get; set; }
    public CaseDocuments? CaseDocument { get; set; }
    
    // FK — Step 4: 1:N (Chat → ChatMessage)
    public int ChatId { get; private set; }
    public Chat Chat { get; set; } = null!;

    // Navigation to Sender
    public User Sender { get; set; } = null!;

    // Parameterless constructor for EF Core
    private ChatMessage() { }

    public static ChatMessage Create(int chatId, int senderId, int receiverId, string content, MessageType messageType, CaseDocuments? document = null)
    {
        var msg = new ChatMessage
        {
            ChatId = chatId,
            SenderId = senderId,
            Content = content,
            MessageType = messageType,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            CaseDocument = document
        };

        msg.AddEvent(new ChatMessageCreatedEvent(chatId, senderId, receiverId, content, messageType));

        return msg;
    }
}

