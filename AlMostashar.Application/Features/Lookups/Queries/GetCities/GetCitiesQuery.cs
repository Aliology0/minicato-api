using AlMostashar.Application.Features.Lookups.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Lookups.Queries.GetCities;

public record GetCitiesQuery(int GovernorateId) : IRequest<Result<IReadOnlyList<CityLookupDto>>>;
