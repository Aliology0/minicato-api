using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.ClientRequests.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.ClientRequests.Queries.GetClientOffers;

public class GetClientOffersQueryHandler : IRequestHandler<GetClientOffersQuery, Result<CursorPagedResult<RequestOfferDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetClientOffersQueryHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<CursorPagedResult<RequestOfferDto>>> Handle(GetClientOffersQuery request, CancellationToken cancellationToken)
    {
        var clientId = _currentUser.UserId;
        var pageSize = Math.Clamp(request.PageSize, 1, 50);

        var query = _db.RequestOffers
            .AsNoTracking()
            .Where(o => o.ClientRequest.ClientId == clientId);

        if (request.RequestId.HasValue)
            query = query.Where(o => o.ClientRequestId == request.RequestId.Value);

        query = query.OrderByDescending(o => o.Id);

        if (request.Cursor.HasValue)
            query = query.Where(o => o.Id < request.Cursor.Value);

        var offers = await query
            .Take(pageSize + 1)
            .Select(o => new RequestOfferDto
            {
                OfferId = o.Id,
                RequestId = o.ClientRequestId,
                RequestTitle = o.ClientRequest.Title,
                LawyerId = o.LawyerId,
                LawyerName = o.Lawyer.FullName,
                LawyerProfileImage = o.Lawyer.AvatarUrl,
                LegalServiceId = o.LegalServiceId,
                LegalServiceTitle = o.LegalService.Title,
                OfferedAmount = o.OfferedAmount,
                Note = o.Note,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                RespondedAt = o.RespondedAt
            })
            .ToListAsync(cancellationToken);

        var hasMore = offers.Count > pageSize;
        if (hasMore)
            offers.RemoveAt(offers.Count - 1);

        return Result<CursorPagedResult<RequestOfferDto>>.Success(new CursorPagedResult<RequestOfferDto>
        {
            Items = offers,
            HasMore = hasMore,
            NextCursor = hasMore && offers.Count > 0 ? offers[^1].OfferId : null,
        });
    }
}
