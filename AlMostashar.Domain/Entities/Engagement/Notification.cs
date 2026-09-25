using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Domain.Entities;

public class Notification : BaseEntity
{
    public int SourceId { get; set; }
    public NotificationType Type { get; set; }
    public string Description { get; set; }
    public string Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }

    // FK — Step 4: 1:N (User → Notification)
    public int UserId { get; set; }
    public User User { get; set; }
}

