using System;
using System.Collections.Generic;
using AlMostashar.Application.Features.Specializations.DTOs;

namespace AlMostashar.Application.Features.Admin.DTOs;

public record UnVerifiedLawyersDto(
    int Id,
    string FullName,
    DateTime CreatedAt,
    string PhoneNo,
    int SyndicateId,
    string? SSN_Url,
    string? SyndicateCardUrl,
    int YearsOfExperience,
    IEnumerable<SpecializationLookupDto> Specializations,
    AlMostashar.Domain.ValueObject.Enum.AccountStatus AccountStatus,
    LawyerProfileReviewDto? PendingChanges);


