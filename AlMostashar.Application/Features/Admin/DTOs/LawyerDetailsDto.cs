using System;
using System.Collections.Generic;
using AlMostashar.Application.Features.Specializations.DTOs;

namespace AlMostashar.Application.Features.Admin.DTOs;

public class LawyerDetailsDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsVerified { get; set; }
    public int YearsOfExperience { get; set; }
    public string? SSN_Url { get; set; }
    public string? SyndicateCardUrl { get; set; }
    public string? PracticeCertificatesUrl { get; set; }
    public int GovernorateId { get; set; }
    public string Governorate { get; set; } = null!;
    public int CityId { get; set; }
    public string City { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public IEnumerable<SpecializationLookupDto> Specializations { get; set; } = Array.Empty<SpecializationLookupDto>();
}
