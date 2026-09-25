namespace AlMostashar.Application.Features.LawyerHome.DTOs;

public class LawyerHomeActiveCaseDto
{
    public int CaseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public DateTime? NextRelevantDate { get; set; }
    public string SecondaryText { get; set; } = string.Empty;
}
