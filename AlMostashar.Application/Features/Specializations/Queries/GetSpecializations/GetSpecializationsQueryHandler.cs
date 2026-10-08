using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Specializations.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Specializations.Queries.GetSpecializations;

public class GetSpecializationsQueryHandler
    : IRequestHandler<GetSpecializationsQuery, Result<IReadOnlyList<SpecializationLookupDto>>>
{
    private readonly IAppDbContext _db;

    public GetSpecializationsQueryHandler(IAppDbContext db) => _db = db;

    public async Task<Result<IReadOnlyList<SpecializationLookupDto>>> Handle(
        GetSpecializationsQuery request,
        CancellationToken cancellationToken)
    {
        var specializations = await _db.LawyerSpecializations
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .Select(s => new SpecializationLookupDto
            {
                Id = s.Id,
                Title = s.Title,
                ArabicTitle = s.ArabicTitle
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<SpecializationLookupDto>>.Success(specializations);
    }
}
