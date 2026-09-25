namespace AlMostashar.Domain.Entities;

public class Lawyer : User
{
    public int YearsOfExperience { get; set; }
    public string PhoneNo { get; set; } = null!;
    public int GovernorateId { get; set; }
    public string Governorate { get; set; } = null!;
    public int CityId { get; set; }
    public string City { get; set; } = null!;
    public int SyndicateId { get; set; }
    public string? Bio { get; set; }
    public string? About { get; set; }
    public string? SSN_Url { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? NotifiedAt { get; set; }
    public string? SyndicateCardUrl { get; set; }
    public string? PracticeCertificatesUrl { get; set; }

    // FK — Step 4: 1:N (Admin → Lawyer via Verifies)
    public int? VerifiedByAdminId { get; set; }
    public Admin? VerifiedByAdmin { get; set; } 

    // Navigation — Step 3: 1:1 (Lawyer → Wallet via Owns)
    public Wallet Wallet { get; set; } = null!;

    // Navigation — Step 5: M:N (Lawyer ↔ Service via LawyerService)
    public ICollection<LawyerService> LawyerServices { get; set; } = new List<LawyerService>();
    public ICollection<RequestOffer> Offers { get; set; } = new List<RequestOffer>();

    // Navigation — 1:N Partial (Lawyer → Cases)
    public ICollection<Case> Cases { get; set; } = new List<Case>();

    // Navigation — M:N (Lawyer ↔ LawyerSpecialization)
    public ICollection<LawyerSpecialization>? LawyerSpecializations { get; set; }
}


