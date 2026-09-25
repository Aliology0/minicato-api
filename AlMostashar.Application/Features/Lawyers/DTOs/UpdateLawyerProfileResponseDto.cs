namespace AlMostashar.Application.Features.Lawyers.DTOs;

public class UpdateLawyerProfileResponseDto
{

    public string? FirstName { get; set; } = null!;
    public string? LastName { get; set; } = null!;
    public string? ProfileImage { get; set; } = null!;
    public List<SpecializationDto>? Specializations { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? Bio { get; set; }
    public string? About { get; set; }
}
