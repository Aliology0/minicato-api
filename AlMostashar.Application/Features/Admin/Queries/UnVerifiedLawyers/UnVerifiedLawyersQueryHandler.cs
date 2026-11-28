using System.Text.Json;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Admin.DTOs;
using AlMostashar.Application.Features.Specializations.DTOs;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Admin.Queries.UnVerifiedLawyers;

public sealed class UnVerifiedLawyersQueryHandler : IRequestHandler<UnVerifiedLawyersQuery, Result<List<UnVerifiedLawyersDto>>>
{
    private readonly IAppDbContext _db;

    public UnVerifiedLawyersQueryHandler(IAppDbContext db) => _db = db;

    public async Task<Result<List<UnVerifiedLawyersDto>>> Handle(UnVerifiedLawyersQuery request, CancellationToken cancellationToken)
    {
        var pendingLawyers = await _db.Lawyers
            .AsNoTracking()
            .Include(l => l.LawyerSpecializations)
            .Include(l => l.PendingProfileChange)
            .Where(l => l.AccountStatus == AccountStatus.PendingReview ||
                        l.AccountStatus == AccountStatus.PendingReverification ||
                        l.AccountStatus == AccountStatus.PendingChangesReview)
            .OrderBy(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

        var results = new List<UnVerifiedLawyersDto>(pendingLawyers.Count);
        foreach (var lawyer in pendingLawyers)
        {
            LawyerProfileReviewDto? pendingChanges = null;
            if (lawyer.PendingProfileChange is { } change)
            {
                var ids = JsonSerializer.Deserialize<int[]>(change.SpecializationIdsJson) ?? Array.Empty<int>();
                var pendingSpecializations = await _db.LawyerSpecializations
                    .Where(s => ids.Contains(s.Id))
                    .Select(s => new SpecializationLookupDto { Id = s.Id, Title = s.Title, ArabicTitle = s.ArabicTitle })
                    .ToListAsync(cancellationToken);
                pendingChanges = new LawyerProfileReviewDto
                {
                    FirstName = change.FirstName,
                    LastName = change.LastName,
                    AvatarUrl = change.AvatarUrl,
                    YearsOfExperience = change.YearsOfExperience,
                    SSN_Url = change.SSN_Url,
                    SyndicateCardUrl = change.SyndicateCardUrl,
                    PhoneNumber = change.PhoneNo,
                    GovernorateId = change.GovernorateId,
                    Governorate = change.Governorate,
                    CityId = change.CityId,
                    City = change.City,
                    SyndicateId = change.SyndicateId,
                    Bio = change.Bio,
                    About = change.About,
                    Specializations = pendingSpecializations
                };
            }

            results.Add(new UnVerifiedLawyersDto(
                lawyer.Id,
                lawyer.FullName,
                lawyer.CreatedAt,
                lawyer.PhoneNo,
                lawyer.SyndicateId,
                lawyer.SSN_Url,
                lawyer.SyndicateCardUrl,
                lawyer.YearsOfExperience,
                lawyer.LawyerSpecializations!
                    .Select(s => new SpecializationLookupDto { Id = s.Id, Title = s.Title, ArabicTitle = s.ArabicTitle })
                    .ToList(),
                lawyer.AccountStatus,
                pendingChanges));
        }

        return Result<List<UnVerifiedLawyersDto>>.Success(results);
    }
}
