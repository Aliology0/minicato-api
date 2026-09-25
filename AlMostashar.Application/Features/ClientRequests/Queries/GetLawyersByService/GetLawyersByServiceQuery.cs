using AlMostashar.Application.Features.ClientRequests.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.ClientRequests.Queries.GetLawyersByService;

public record GetLawyersByServiceQuery(
    int LegalServiceId,
    int? GovernorateId = null,
    int? CityId = null)
    : IRequest<Result<List<LawyerWithPriceDto>>>;
