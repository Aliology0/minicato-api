using AlMostashar.Application.Features.ClientRequests.DTOs;
using AlMostashar.Application.Common.Models;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;

namespace AlMostashar.Application.Features.ClientRequests.Queries.GetBroadcastRequests;

public record GetBroadcastRequestsQuery(
    decimal? MinBudget = null,
    decimal? MaxBudget = null,
    ServiceType? ServiceType = null,
    int? GovernorateId = null,
    int? CityId = null,
    int? Cursor = null,
    int PageSize = 10)
    : IRequest<Result<CursorPagedResult<BroadcastRequestDto>>>;
