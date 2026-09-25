namespace AlMostashar.Domain.Entities;

// Step 2: Weak Entity — PK is composite (ChatId + UserId)
public class ChatParticipant 
{
    public int ChatId { get; set; }
    public Chat Chat { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int UnReadMessageCount { get; set; }
}
