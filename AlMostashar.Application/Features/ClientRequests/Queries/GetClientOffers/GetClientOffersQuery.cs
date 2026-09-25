using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.ClientRequests.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.ClientRequests.Queries.GetClientOffers;

public record GetClientOffersQuery(
    int? RequestId = null,
    int? Cursor = null,
    int PageSize = 10)
    : IRequest<Result<CursorPagedResult<RequestOfferDto>>>;
