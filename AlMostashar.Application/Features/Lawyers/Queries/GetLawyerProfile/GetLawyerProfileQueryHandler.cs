using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Lawyers.DTOs;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Lawyers.Queries.GetLawyerProfile;

public class GetLawyerProfileQueryHandler
    : IRequestHandler<GetLawyerProfileQuery, Result<LawyerProfileDto>>
{
    private const int MaxFeedbacks = 5;

    private readonly IAppDbContext _db;

    public GetLawyerProfileQueryHandler(IAppDbContext db) => _db = db;

    public async Task<Result<LawyerProfileDto>> Handle(
        GetLawyerProfileQuery request, CancellationToken cancellationToken)
    {
        var lawyerId = request.LawyerId;

        var profile = await _db.Lawyers
            .AsNoTracking()
            .Where(l => l.Id == lawyerId
                     && l.AccountStatus == AccountStatus.Active
                     && l.IsVerified)
            .Select(l => new
            {
                LawyerId = l.Id,
                ProfileImage = l.AvatarUrl,
                FullName = l.FullName,
                YearsOfExperience = l.YearsOfExperience,
                Bio =  l.Bio,
                About=l.About,

                // Rating aggregates
                RatingsCount = _db.Feedbacks.Count(f => f.ClientRequest.LawyerServiceLawyerId == lawyerId),
                RatingSum = _db.Feedbacks
                                .Where(f => f.ClientRequest.LawyerServiceLawyerId == lawyerId)
                                .Sum(f => (double?)f.Rate) ?? 0.0,

                // Served clients
                ServedClientsCount = _db.ClientRequests
                    .Where(cr => cr.LawyerServiceLawyerId == lawyerId
                              && (cr.Status == ClientRequestStatus.Completed
                               || cr.Status == ClientRequestStatus.InProgress))
                    .Select(cr => cr.ClientId)
                    .Distinct()
                    .Count(),

                // Completed cases
                CompletedCasesCount = _db.Cases
                    .Count(c => c.LawyerId == lawyerId && c.Status == CaseStatus.Closed),

                // Collections
                Specializations = l.LawyerSpecializations!
                    .Select(s => new SpecializationDto(s.Id, s.Title, s.ArabicTitle))
                    .ToList(),

                Services = l.LawyerServices
                    .Where(ls => ls.IsActive && ls.LegalService.IsActive)
                    .Select(ls => new LawyerServiceItemDto
                    {
                        ServiceId = ls.LegalServiceId,
                        Title = ls.LegalService.Title,
                        Price = ls.Price,
                        Duration = ls.Duration,
                    })
                    .ToList(),

                TopFeedbacks = _db.Feedbacks
                    .Where(f => f.ClientRequest.LawyerServiceLawyerId == lawyerId)
                    .OrderByDescending(f => f.ClientRequest.CreatedAt)
                    .ThenByDescending(f => f.Id)
                    .Take(MaxFeedbacks)
                    .Select(f => new FeedbackItemDto
                    {
                        ClientProfilePhoto = f.ClientRequest.Client.AvatarUrl,
                        FirstName = f.ClientRequest.Client.FirstName,
                        LastName = f.ClientRequest.Client.LastName,
                        Rate = f.Rate,
                        CreatedAt = f.ClientRequest.CreatedAt,
                        FeedbackContent = f.Content,
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
            return Result<LawyerProfileDto>.Failure(
                new Error("Lawyer.NotFound", "Lawyer not found or profile is not public."));

        var ratingAverage = profile.RatingsCount > 0
            ? Math.Round(profile.RatingSum / profile.RatingsCount, 1)
            : (double?)null;

        var dto = new LawyerProfileDto
        {
            LawyerId = profile.LawyerId,
            ProfileImage = profile.ProfileImage,
            FullName = profile.FullName,
            RatingAverage = ratingAverage??0,
            RatingsCount = profile.RatingsCount,
            YearsOfExperience = profile.YearsOfExperience,
            ServedClientsCount = profile.ServedClientsCount,
            CompletedCasesCount = profile.CompletedCasesCount,
            Bio = profile.Bio,
            About = profile.About,
            Specializations = profile.Specializations,
            Services = profile.Services,
            TopFeedbacks = profile.TopFeedbacks,
        };

        return Result<LawyerProfileDto>.Success(dto);
    }
}
