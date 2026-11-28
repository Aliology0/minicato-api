namespace AlMostashar.Application.Features.Lawyers.DTOs;

public class UpdateLawyerProfileResponseDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfileImage { get; set; }
    public string? PhoneNumber { get; set; }
    public int? GovernorateId { get; set; }
    public string? Governorate { get; set; }
    public int? CityId { get; set; }
    public string? City { get; set; }
    public int? SyndicateId { get; set; }
    public List<SpecializationDto>? Specializations { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? Bio { get; set; }
    public string? About { get; set; }
    public AlMostashar.Domain.ValueObject.Enum.AccountStatus AccountStatus { get; set; }
}
