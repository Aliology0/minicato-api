using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Locations;
using AlMostashar.Application.Features.ClientRequests.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Xml;

namespace AlMostashar.Application.Features.ClientRequests.Queries.GetLawyersByService;

public class GetLawyersByServiceQueryHandler
    : IRequestHandler<GetLawyersByServiceQuery, Result<List<LawyerWithPriceDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ILocationCatalog _locationCatalog;

    public GetLawyersByServiceQueryHandler(IAppDbContext db, ILocationCatalog locationCatalog)
    {
        _db = db;
        _locationCatalog = locationCatalog;
    }

    public async Task<Result<List<LawyerWithPriceDto>>> Handle(
        GetLawyersByServiceQuery request, CancellationToken cancellationToken)
    {
        var locationFilterResult = RequestLocationResolver.ResolveOptional(
            _locationCatalog,
            request.GovernorateId,
            request.CityId);

        if (!locationFilterResult.IsSuccess)
            return Result<List<LawyerWithPriceDto>>.Failure(locationFilterResult.Error!);
        var locationFilter = locationFilterResult.Value!;

        var query = _db.LawyerServices
            .AsNoTracking()
            .Where(ls => ls.LegalServiceId == request.LegalServiceId && ls.IsActive)
            .Where(ls => ls.Lawyer.AccountStatus == AlMostashar.Domain.ValueObject.Enum.AccountStatus.Active);

        if (locationFilter.GovernorateName is not null)
        {
            query = query.Where(ls =>
                ls.Lawyer.Governorate == locationFilter.GovernorateName ||
                ls.Lawyer.Governorate == locationFilter.GovernorateEnglishName);
        }

        if (locationFilter.CityName is not null)
        {
            query = query.Where(ls =>
                ls.Lawyer.City == locationFilter.CityName ||
                ls.Lawyer.City == locationFilter.CityEnglishName);
        }

        var lawyers = await query
            .Select(ls => new LawyerWithPriceDto
            {
                LawyerId = ls.LawyerId,
                FullName = ls.Lawyer.FullName,
                ProfileImage = ls.Lawyer.AvatarUrl,
                Specialization = ls.Lawyer.LawyerSpecializations!.Select(x=>x.ArabicTitle).FirstOrDefault(),//temporarily till we figure out what to do
                Price = ls.Price,
                Duration = ls.Duration,
            })
            .ToListAsync(cancellationToken);

        return Result<List<LawyerWithPriceDto>>.Success(lawyers);
    }
}
