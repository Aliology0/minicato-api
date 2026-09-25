namespace AlMostashar.Application.Features.Lawyers.DTOs;

public class LawyerProfileDto
{
    public int LawyerId { get; set; }
    public string? ProfileImage { get; set; }
    public string FullName { get; set; } = string.Empty;
    public double? RatingAverage { get; set; } = 0;
    public int? RatingsCount { get; set; } = 0;
    public int? YearsOfExperience { get; set; }
    public int ServedClientsCount { get; set; } = 0;
    public int CompletedCasesCount { get; set; } = 0;
    public string? Bio { get; set; }
    public string? About { get; set; }
    public List<SpecializationDto>? Specializations { get; set; }
    public List<LawyerServiceItemDto>? Services { get; set; } 
    public List<FeedbackItemDto>? TopFeedbacks { get; set; } 
}

public class LawyerServiceItemDto
{
    public int ServiceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Duration { get; set; } = string.Empty;
}

public class FeedbackItemDto
{
    public string? ClientProfilePhoto { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public double Rate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string FeedbackContent { get; set; } = string.Empty;
}
