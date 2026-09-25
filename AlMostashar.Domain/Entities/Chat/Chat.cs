using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Domain.Entities;

public class Chat : BaseEntity
{
    public Chat(int CaseId)
    {
        this.CaseId = CaseId;
        CreatedAt = DateTime.UtcNow;
        Status = ChatStatus.Active;
    }
    private Chat()
    {
    }

    public DateTime CreatedAt { get; private set; }
    public int? LastMessageSenderId { get; private set; }
    public string? LastMessageContent { get; private set; }
    public MessageType? LastMessageType { get; private set; }
    public DateTime? LastMessageAt { get; private set; }
    public ChatStatus Status { get; private set; }

    // Navigation — Step 4: 1:N (Chat → ChatMessage)
    public ICollection<ChatMessage> ChatMessages { get; private set; } = new List<ChatMessage>();

    // Navigation — Step 2: Weak Entity (Chat → ChatParticipant)
    public ICollection<ChatParticipant> ChatParticipants { get; private set; } = new List<ChatParticipant>();
    // Navigation — Step 2: Weak Entity (Chat → ChatParticipant)
    public ICollection<CallSession> CallSessions { get; private set; } = new List<CallSession>();

    // FK — 1:1 Total (Chat must belong to a Case)
    public int CaseId { get; private set; }
    public Case Case { get; private set; } = null!;

    // 3. Factory method or Public Constructor for explicit creation
    public static Chat Create(int CaseId, int Participant1Id, int Participant2Id)
    {
        var chat = new Chat(CaseId);
        chat.AddParticipant(Participant1Id);
        chat.AddParticipant(Participant2Id);
        return chat;
    }
    public void updateLastMessage(int lastMessageSenderI, string lastMessageContent, MessageType messageType, DateTime LastMessageAt)
    {
        this.LastMessageSenderId = lastMessageSenderI;
        this.LastMessageContent = lastMessageContent;
        this.LastMessageType = messageType;
        this.LastMessageAt = LastMessageAt;
    }
    // 4. Domain Method to add participants (Centralized Business Logic)
    public void AddParticipant(int userId)
    {
        // Protect your domain invariants here
        if (ChatParticipants.Any(p => p.UserId == userId))
        {
            throw new InvalidOperationException("User is already a participant in this chat.");
        }

        var participant = new ChatParticipant { UserId = userId };
        ChatParticipants.Add(participant);
    }
}
