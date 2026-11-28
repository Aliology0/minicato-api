namespace AlMostashar.Domain.Entities;

public sealed class LawyerProfileChange
{
    public int Id { get; set; }
    public int LawyerId { get; set; }
    public Lawyer Lawyer { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public int YearsOfExperience { get; set; }
    public string? SSN_Url { get; set; }
    public string? SyndicateCardUrl { get; set; }
    public string PhoneNo { get; set; } = null!;
    public int GovernorateId { get; set; }
    public string Governorate { get; set; } = null!;
    public int CityId { get; set; }
    public string City { get; set; } = null!;
    public int SyndicateId { get; set; }
    public string? Bio { get; set; }
    public string? About { get; set; }
    public string SpecializationIdsJson { get; set; } = "[]";
}
