using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Lawyers.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Lawyers.Queries.GetLawyerSpecializations;

public class GetLawyerSpecializationsQueryHandler
    : IRequestHandler<GetLawyerSpecializationsQuery, Result<IEnumerable<LawyerSpecializationLookupDto>>>
{
    private readonly IAppDbContext _db;

    public GetLawyerSpecializationsQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<IEnumerable<LawyerSpecializationLookupDto>>> Handle(
        GetLawyerSpecializationsQuery request, CancellationToken cancellationToken)
    {
        var specializations = await _db.LawyerSpecializations
            .Select(s => new LawyerSpecializationLookupDto
            {
                Id = s.Id,
                Title = s.Title,
                ArabicTitle = s.ArabicTitle
            })
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<LawyerSpecializationLookupDto>>.Success(specializations);
    }
}
