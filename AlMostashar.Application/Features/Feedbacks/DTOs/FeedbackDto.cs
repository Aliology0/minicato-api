namespace AlMostashar.Application.Features.Feedbacks.DTOs;

public class FeedbackDto
{
    public int Id { get; set; }
    public string ClientProfilePhoto { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public double Rate { get; set; }
    public DateTime CreateAt { get; set; }
    public string FeedbackContent { get; set; } = string.Empty;
}
