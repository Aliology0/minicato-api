using AlMostashar.Application.Common.Locations;
using AlMostashar.Application.Features.Lookups.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Lookups.Queries.GetGovernorates;

public class GetGovernoratesQueryHandler
    : IRequestHandler<GetGovernoratesQuery, Result<IReadOnlyList<GovernorateLookupDto>>>
{
    private readonly ILocationCatalog _locationCatalog;

    public GetGovernoratesQueryHandler(ILocationCatalog locationCatalog) => _locationCatalog = locationCatalog;

    public Task<Result<IReadOnlyList<GovernorateLookupDto>>> Handle(
        GetGovernoratesQuery request,
        CancellationToken cancellationToken)
    {
        var governorates = _locationCatalog
            .GetGovernorates()
            .OrderBy(g => g.Id)
            .Select(g => new GovernorateLookupDto
            {
                Id = g.Id,
                Name = g.Name
            })
            .ToList();
        return Task.FromResult(Result<IReadOnlyList<GovernorateLookupDto>>.Success(governorates));
    }
}
