using AlMostashar.Application.Common.Locations;
using AlMostashar.Application.Common.Models;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.ClientRequests.DTOs;
using AlMostashar.Application.Features.ClientRequests.Queries;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.ClientRequests.Queries.GetBroadcastRequests;

public class GetBroadcastRequestsQueryHandler
    : IRequestHandler<GetBroadcastRequestsQuery, Result<CursorPagedResult<BroadcastRequestDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ILocationCatalog _locationCatalog;

    public GetBroadcastRequestsQueryHandler(IAppDbContext db, ILocationCatalog locationCatalog)
    {
        _db = db;
        _locationCatalog = locationCatalog;
    }

    public async Task<Result<CursorPagedResult<BroadcastRequestDto>>> Handle(
        GetBroadcastRequestsQuery request, CancellationToken cancellationToken)
    {
        var locationFilterResult = RequestLocationResolver.ResolveOptional(
            _locationCatalog,
            request.GovernorateId,
            request.CityId);

        if (!locationFilterResult.IsSuccess)
            return Result<CursorPagedResult<BroadcastRequestDto>>.Failure(locationFilterResult.Error!);
        var locationFilter = locationFilterResult.Value!;

        var pageSize = Math.Clamp(request.PageSize, 1, 50);

        var query = _db.BroadcastRequests
            .AsNoTracking()
            .Where(r => r.Status == ClientRequestStatus.Pending)
            .AsQueryable();

        if (request.MinBudget.HasValue)
            query = query.Where(r => r.Budget >= request.MinBudget.Value);

        if (request.MaxBudget.HasValue)
            query = query.Where(r => r.Budget <= request.MaxBudget.Value);

        if (request.ServiceType.HasValue)
        {
            query = query.Where(
                r => r.RequestedLegalService != null &&
                     r.RequestedLegalService.ServiceType == request.ServiceType.Value);
        }

        if (locationFilter.GovernorateId.HasValue)
            query = query.Where(r => r.GovernorateId == locationFilter.GovernorateId.Value);

        if (locationFilter.CityId.HasValue)
            query = query.Where(r => r.CityId == locationFilter.CityId.Value);

        // ── Cursor pagination (ordered by Id descending) ────────────────

        query = query.OrderByDescending(r => r.Id);

        if (request.Cursor.HasValue)
            query = query.Where(r => r.Id < request.Cursor.Value);

        var rows = await query
            .Take(pageSize + 1)
            .Select(r => new
            {
                r.Id,
                r.RequestId,
                r.Title,
                r.ProblemDetails,
                Status = r.Status.ToString(),
                r.LegalServiceId,
                r.ServiceType,
                r.ClientDeadline,
                r.PreferredCommunicationMethod,
                r.Urgency,
                ServiceTitle = r.RequestedLegalService != null ? r.RequestedLegalService.Title : null,
                r.GovernorateId,
                r.Governorate,
                r.CityId,
                r.City,
                r.Budget,
                ClientName = r.Client.FullName,
                r.CreatedAt,
                r.Documents
            })
            .ToListAsync(cancellationToken);

        var hasMore = rows.Count > pageSize;
        if (hasMore)
            rows.RemoveAt(rows.Count - 1);

        var detailsByRequestId = await RequestDetailsDataLoader.LoadAsync(_db, rows.Select(r => r.Id).ToList(), cancellationToken);

        var items = rows.Select(r => new BroadcastRequestDto
        {
            Id = r.Id,
            RequestId = r.RequestId,
            Title = r.Title,
            ProblemDetails = r.ProblemDetails,
            Status = r.Status,
            LegalServiceId = r.LegalServiceId,
            ServiceTitle = r.ServiceTitle,
            GovernorateId = r.GovernorateId,
            Governorate = r.Governorate,
            CityId = r.CityId,
            City = r.City,
            Budget = r.Budget,
            ClientName = r.ClientName,
            Documents = r.Documents.Select(d=> new DocumentDto { Id= d.Id, DocumentName= d.DocumentName, Type= d.DocumentType, SizeInBytes = d.SizeInBytes, CreatedAt = d.CreatedAt}).ToList(),
            CreatedAt = r.CreatedAt,
            ServiceType = r.ServiceType,
            ClientDeadline = r.ClientDeadline,
            PreferredCommunicationMethod = r.PreferredCommunicationMethod,
            Urgency = r.Urgency,
            RequestDetails = detailsByRequestId.GetValueOrDefault(r.Id),
        }).ToList();

        return Result<CursorPagedResult<BroadcastRequestDto>>.Success(new CursorPagedResult<BroadcastRequestDto>
        {
            Items = items,
            HasMore = hasMore,
            NextCursor = hasMore && items.Count > 0 ? items[^1].Id : null,
        });
    }
}
