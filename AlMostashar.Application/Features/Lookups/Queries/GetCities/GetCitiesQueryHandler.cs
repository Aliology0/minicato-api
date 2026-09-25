using AlMostashar.Application.Common.Locations;
using AlMostashar.Application.Features.Lookups.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Lookups.Queries.GetCities;

public class GetCitiesQueryHandler
    : IRequestHandler<GetCitiesQuery, Result<IReadOnlyList<CityLookupDto>>>
{
    private readonly ILocationCatalog _locationCatalog;

    public GetCitiesQueryHandler(ILocationCatalog locationCatalog) => _locationCatalog = locationCatalog;

    public Task<Result<IReadOnlyList<CityLookupDto>>> Handle(
        GetCitiesQuery request,
        CancellationToken cancellationToken)
    {
        var cities = _locationCatalog
            .GetCities(request.GovernorateId)
            .OrderBy(c => c.Name)
            .Select(c => new CityLookupDto
            {
                Id = c.Id,
                GovernorateId = c.GovernorateId,
                Name = c.Name
            })
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<CityLookupDto>>.Success(cities));
    }
}
