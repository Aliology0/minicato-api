using System.Text.Json;
using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Admin.DTOs;
using AlMostashar.Application.Features.Specializations.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Admin.Queries.GetLawyerById;

public sealed class GetLawyerByIdQueryHandler : IRequestHandler<GetLawyerByIdQuery, Result<LawyerDetailsDto>>
{
    private readonly IAppDbContext _db;

    public GetLawyerByIdQueryHandler(IAppDbContext db) => _db = db;

    public async Task<Result<LawyerDetailsDto>> Handle(GetLawyerByIdQuery request, CancellationToken cancellationToken)
    {
        var lawyer = await _db.Lawyers
            .AsNoTracking()
            .Include(l => l.LawyerSpecializations)
            .Include(l => l.PendingProfileChange)
            .SingleOrDefaultAsync(l => l.Id == request.Id, cancellationToken);
        if (lawyer is null)
            return Result<LawyerDetailsDto>.Failure(new Error("Lawyer.NotFound", Messages.Generic.NotFound("Lawyer")));

        var currentSpecializations = lawyer.LawyerSpecializations!
            .Select(s => new SpecializationLookupDto { Id = s.Id, Title = s.Title, ArabicTitle = s.ArabicTitle })
            .ToList();
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

        return Result<LawyerDetailsDto>.Success(new LawyerDetailsDto
        {
            Id = lawyer.Id,
            UserId = lawyer.Id,
            FirstName = lawyer.FirstName,
            LastName = lawyer.LastName,
            FullName = lawyer.FullName,
            AvatarUrl = lawyer.AvatarUrl,
            Email = lawyer.Email,
            PhoneNumber = lawyer.PhoneNo,
            SyndicateId = lawyer.SyndicateId,
            IsVerified = lawyer.IsVerified,
            YearsOfExperience = lawyer.YearsOfExperience,
            SSN_Url = lawyer.SSN_Url,
            SyndicateCardUrl = lawyer.SyndicateCardUrl,
            GovernorateId = lawyer.GovernorateId,
            Governorate = lawyer.Governorate,
            CityId = lawyer.CityId,
            City = lawyer.City,
            CreatedAt = lawyer.CreatedAt,
            AccountStatus = lawyer.AccountStatus,
            RejectionReason = lawyer.RejectionReason,
            Specializations = currentSpecializations,
            PendingChanges = pendingChanges
        });
    }
}
