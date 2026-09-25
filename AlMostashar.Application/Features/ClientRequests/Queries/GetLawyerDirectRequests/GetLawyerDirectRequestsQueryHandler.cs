using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.ClientRequests.DTOs;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.ClientRequests.Queries.GetLawyerDirectRequests;

public class GetLawyerDirectRequestsQueryHandler
    : IRequestHandler<GetLawyerDirectRequestsQuery, Result<List<LawyerDirectRequestDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetLawyerDirectRequestsQueryHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<List<LawyerDirectRequestDto>>> Handle(
        GetLawyerDirectRequestsQuery request, CancellationToken cancellationToken)
    {
        var lawyerId = _currentUser.UserId;

        var query = _db.DirectRequests
            .AsNoTracking()
            .Where(r => r.LawyerServiceLawyerId == lawyerId)
            .AsQueryable();

        if (request.Status is not null)
            query = query.Where(r => r.Status == request.Status);

        if (request.ServiceType is not null)
        {
            query = query.Where(
                r => r.RequestedLegalService != null &&
                     r.RequestedLegalService.ServiceType == request.ServiceType);
        }

        var rows = await query
            .OrderByDescending(r => r.CreatedAt)
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
                Type = "Direct",
                ServiceTitle = r.RequestedLegalService != null ? r.RequestedLegalService.Title : null,
                Price = r.LawyersServices != null ? r.LawyersServices.Price : (decimal?)null,

                r.ClientId,
                ClientName = r.Client.FullName,
                ClientProfileImage = r.Client.AvatarUrl,

                r.GovernorateId,
                r.Governorate,
                r.CityId,
                r.City,

                r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var documentsByRequestId = await RequestQueryDataLoader.LoadDocumentsByRequestIdsAsync(
            _db,
            rows.Select(r => r.Id).ToList(),
            cancellationToken);
        var detailsByRequestId = await RequestDetailsDataLoader.LoadAsync(_db, rows.Select(r => r.Id).ToList(), cancellationToken);

        var results = rows.Select(r => new LawyerDirectRequestDto
        {
            Id = r.Id,
            RequestId = r.RequestId,
            Title = r.Title,
            ProblemDetails = r.ProblemDetails,
            Status = r.Status,
            Type = r.Type,
            ServiceTitle = r.ServiceTitle,
            Price = r.Price,
            Documents = documentsByRequestId.GetValueOrDefault(r.Id, []),
            CreatedAt = r.CreatedAt,
            LegalServiceId = r.LegalServiceId,
            ServiceType = r.ServiceType,
            ClientDeadline = r.ClientDeadline,
            PreferredCommunicationMethod = r.PreferredCommunicationMethod,
            Urgency = r.Urgency,
            RequestDetails = detailsByRequestId.GetValueOrDefault(r.Id),
            ClientId = r.ClientId,
            ClientName = r.ClientName,
            ClientProfileImage = r.ClientProfileImage,
            Location = new LocationDto
            {
                GovernorateId = r.GovernorateId,
                Governorate = r.Governorate,
                CityId = r.CityId,
                City = r.City,
            },
        }).ToList();

        return Result<List<LawyerDirectRequestDto>>.Success(results);
    }
}
