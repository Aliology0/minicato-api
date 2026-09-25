using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.ClientRequests.DTOs;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;

namespace AlMostashar.Application.Features.ClientRequests.Queries.GetMyRequests;

public record GetMyRequestsQuery(
    ClientRequestStatus? Status = null,
    ServiceType? ServiceType = null,
    int? Cursor = null,
    int PageSize = 10)
    : IRequest<Result<CursorPagedResult<ClientRequestDto>>>;
