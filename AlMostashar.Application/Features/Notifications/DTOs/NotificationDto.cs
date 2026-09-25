namespace AlMostashar.Application.Features.Notifications.DTOs;

public class NotificationDto
{
    public int Id { get; set; }
    public int SourceId { get; set; }
    public string Type { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}
