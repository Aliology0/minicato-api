using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Admin.DTOs;
using AlMostashar.Application.Features.Specializations.DTOs;
using AlMostashar.Domain.Shared;
using AlMostashar.Application.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Admin.Queries.GetLawyerById;

public class GetLawyerByIdQueryHandler : IRequestHandler<GetLawyerByIdQuery, Result<LawyerDetailsDto>>
{
    private readonly IAppDbContext _db;

    public GetLawyerByIdQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<LawyerDetailsDto>> Handle(GetLawyerByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _db.Lawyers
            .Where(l => l.Id == request.Id)
            .Select(l => new LawyerDetailsDto
            {
                Id = l.Id,
                UserId = l.Id,
                FirstName = l.FirstName,
                LastName = l.LastName,
                FullName = l.FullName,
                Email = l.Email,
                PhoneNumber = l.PhoneNo,
                SyndicateId = l.SyndicateId,
                IsVerified = l.IsVerified,
                YearsOfExperience = l.YearsOfExperience,
                SSN_Url = l.SSN_Url,
                SyndicateCardUrl = l.SyndicateCardUrl,
                PracticeCertificatesUrl = l.PracticeCertificatesUrl,
                GovernorateId = l.GovernorateId,
                Governorate = l.Governorate,
                CityId = l.CityId,
                City = l.City,
                CreatedAt = l.CreatedAt,
                Specializations = l.LawyerSpecializations!
                    .Select(s => new SpecializationLookupDto { Id = s.Id, Title = s.Title, ArabicTitle = s.ArabicTitle })
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (dto is null)
            return Result<LawyerDetailsDto>.Failure(new AlMostashar.Domain.Shared.Error("Lawyer.NotFound", Messages.Generic.NotFound("Lawyer")));

        return Result<LawyerDetailsDto>.Success(dto);
    }
}
