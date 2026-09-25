namespace AlMostashar.Domain.ValueObject;

public class DisputeChatMessage
{
    public int SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}
