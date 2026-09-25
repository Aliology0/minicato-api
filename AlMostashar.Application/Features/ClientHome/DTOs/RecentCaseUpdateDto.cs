namespace AlMostashar.Application.Features.ClientHome.DTOs;

public class RecentCaseUpdateDto
{
    public int CaseId { get; set; }
    public string CaseTitle { get; set; } = string.Empty;
    public string UpdateType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
