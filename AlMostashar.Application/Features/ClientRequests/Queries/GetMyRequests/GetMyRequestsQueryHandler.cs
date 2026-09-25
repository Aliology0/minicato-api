using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Features.ClientRequests.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.ClientRequests.Queries.GetMyRequests;

public class GetMyRequestsQueryHandler
    : IRequestHandler<GetMyRequestsQuery, Result<CursorPagedResult<ClientRequestDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMyRequestsQueryHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<CursorPagedResult<ClientRequestDto>>> Handle(
        GetMyRequestsQuery request, CancellationToken cancellationToken)
    {
        var clientId = _currentUser.UserId;
        var pageSize = Math.Clamp(request.PageSize, 1, 50);

        // ── Single query on base ClientRequests table (TPH) ─────────────

        var query = _db.ClientRequests
            .AsNoTracking()
            .Where(r => r.ClientId == clientId);

        if (request.Status is not null)
            query = query.Where(r => r.Status == request.Status);

        if (request.ServiceType is not null)
            query = query.Where(
                r => r.RequestedLegalService != null &&
                     r.RequestedLegalService.ServiceType == request.ServiceType);

        // ── Cursor pagination (ordered by Id descending) ────────────────

        query = query.OrderByDescending(r => r.Id);

        if (request.Cursor.HasValue)
            query = query.Where(r => r.Id < request.Cursor.Value);

        // ── Project + fetch one extra for HasMore detection ─────────────

        var rows = await query
            .Take(pageSize + 1)
            .Select(r => new
            {
                r.Id,
                r.RequestId,
                r.Title,
                r.ProblemDetails,
                r.Status,
                r.LegalServiceId,
                r.ServiceType,
                r.ClientDeadline,
                r.PreferredCommunicationMethod,
                r.Urgency,
                IsBroadcast = r is BroadcastRequest,
                ServiceTitle = r.RequestedLegalService != null ? r.RequestedLegalService.Title : null,
                Price = r.LawyersServices != null ? r.LawyersServices.Price : (decimal?)null,
                Budget = r is BroadcastRequest ? ((BroadcastRequest)r).Budget : (decimal?)null,

                LawyerId = r.LawyerServiceLawyerId,
                LawyerName = r.LawyersServices != null ? r.LawyersServices.Lawyer.FullName : null,
                LawyerProfileImage = r.LawyersServices != null ? r.LawyersServices.Lawyer.AvatarUrl : null,

                r.GovernorateId,
                r.Governorate,
                r.CityId,
                r.City,

                InvoiceId = r.Invoice != null ? (int?)r.Invoice.Id : null,
                InvoiceStatus = r.Invoice != null ? (InvoiceStatus?)r.Invoice.Status : null,
                InvoiceTotalAmount = r.Invoice != null ? (decimal?)r.Invoice.TotalAmount : null,
                InvoiceReferenceNumber = r.Invoice != null ? r.Invoice.ReferenceNumber : null,
                InvoiceServiceTitle = r.RequestedLegalService != null ? r.RequestedLegalService.Title : null,

                r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var hasMore = rows.Count > pageSize;
        if (hasMore)
            rows.RemoveAt(rows.Count - 1);

        // ── Batch-load offer counts and documents ───────────────────────

        var requestIds = rows.Select(r => r.Id).ToList();

        var offerCounts = await RequestQueryDataLoader.LoadOfferCountsByRequestIdsAsync(_db, requestIds, cancellationToken);
        var documentsByRequestId = await RequestQueryDataLoader.LoadDocumentsByRequestIdsAsync(_db, requestIds, cancellationToken);
        var detailsByRequestId = await RequestDetailsDataLoader.LoadAsync(_db, requestIds, cancellationToken);

        // ── Assemble DTOs ───────────────────────────────────────────────

        var items = rows.Select(r => new ClientRequestDto
        {
            Id = r.Id,
            RequestId = r.RequestId,
            Title = r.Title,
            ProblemDetails = r.ProblemDetails,
            Status = r.Status,
            Type = r.IsBroadcast ? "Broadcast" : "Direct",
            ServiceTitle = r.ServiceTitle,
            Price = r.Price,
            Budget = r.Budget,
            OfferCount = offerCounts.GetValueOrDefault(r.Id),
            Documents = documentsByRequestId.GetValueOrDefault(r.Id, []),
            CreatedAt = r.CreatedAt,
            LegalServiceId = r.LegalServiceId,
            ServiceType = r.ServiceType,
            ClientDeadline = r.ClientDeadline,
            PreferredCommunicationMethod = r.PreferredCommunicationMethod,
            Urgency = r.Urgency,
            RequestDetails = detailsByRequestId.GetValueOrDefault(r.Id),
            LawyerId = r.LawyerId,
            LawyerName = r.LawyerName,
            LawyerProfileImage = r.LawyerProfileImage,
            Location = new LocationDto
            {
                GovernorateId = r.GovernorateId,
                Governorate = r.Governorate,
                CityId = r.CityId,
                City = r.City,
            },
            Invoice = r.InvoiceId != null
                ? new InvoiceDto
                {
                    Id = (int)r.InvoiceId,
                    Status = (InvoiceStatus)r.InvoiceStatus!,
                    TotalAmount = (decimal)r.InvoiceTotalAmount!,
                    ReferenceNumber = r.InvoiceReferenceNumber!,
                    ServiceTitle = r.InvoiceServiceTitle,
                }
                : null,
        }).ToList();

        var result = new CursorPagedResult<ClientRequestDto>
        {
            Items = items,
            HasMore = hasMore,
            NextCursor = hasMore && items.Count > 0 ? items[^1].Id : null,
        };

        return Result<CursorPagedResult<ClientRequestDto>>.Success(result);
    }
}
