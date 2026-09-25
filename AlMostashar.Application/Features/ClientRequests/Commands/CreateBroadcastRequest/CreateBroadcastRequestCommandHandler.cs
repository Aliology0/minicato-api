using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Locations;
using AlMostashar.Application.Features.ClientRequests.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.ClientRequests.Commands.CreateBroadcastRequest;

public class CreateBroadcastRequestCommandHandler
    : IRequestHandler<CreateBroadcastRequestCommand, Result<CreateRequestResponseDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILocationCatalog _locationCatalog;
    private readonly IRequestDetailsService _requestDetailsService;
    private readonly IClientRequestAttachmentService _attachmentService;

    public CreateBroadcastRequestCommandHandler(
        IAppDbContext db,
        ICurrentUserService currentUser,
        ILocationCatalog locationCatalog,
        IRequestDetailsService requestDetailsService,
        IClientRequestAttachmentService attachmentService)
    {
        _db = db;
        _currentUser = currentUser;
        _locationCatalog = locationCatalog;
        _requestDetailsService = requestDetailsService;
        _attachmentService = attachmentService;
    }

    public async Task<Result<CreateRequestResponseDto>> Handle(
        CreateBroadcastRequestCommand request, CancellationToken cancellationToken)
    {
        var locationResult = RequestLocationResolver.ResolveRequired(
            _locationCatalog,
            request.GovernorateId,
            request.CityId);

        if (!locationResult.IsSuccess)
            return Result<CreateRequestResponseDto>.Failure(locationResult.Error!);
        var location = locationResult.Value!;

        var legalService = await _db.LegalServices
            .AsNoTracking()
            .FirstOrDefaultAsync(
                ls => ls.Id == request.LegalServiceId && ls.IsActive,
                cancellationToken);

        if (legalService is null)
        {
            return Result<CreateRequestResponseDto>.Failure(
                new Error("Request.InvalidLegalService", Messages.Generic.NotFound("Active LegalService")));
        }

        var detailsResult = _requestDetailsService.ValidateAndNormalize(
            legalService.ServiceType, request.RequestDetails);
        if (!detailsResult.IsSuccess)
            return Result<CreateRequestResponseDto>.Failure(detailsResult.Error!);

        var broadcastRequest = new BroadcastRequest
        {
            RequestId = $"REQ-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}",
            Title = request.Title,
            ProblemDetails = request.ProblemDetails,
            GovernorateId = location.GovernorateId,
            Governorate = location.Governorate,
            CityId = location.CityId,
            City = location.City,
            Budget = request.Budget,
            ClientId = _currentUser.UserId,
            LegalServiceId = legalService.Id,
            ServiceType = legalService.ServiceType,
            ClientDeadline = request.ClientDeadline,
            PreferredCommunicationMethod = request.PreferredCommunicationMethod,
            Urgency = request.Urgency,
            LawyerServiceLegalServiceId = request.LegalServiceId,
            Status = ClientRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        _requestDetailsService.AttachToRequest(broadcastRequest, detailsResult.Value!.Details);

        _db.BroadcastRequests.Add(broadcastRequest);
        var attachmentResult = await _attachmentService.LinkAsync(
            broadcastRequest, request.AttachmentIds, _currentUser.UserId, cancellationToken);
        if (!attachmentResult.IsSuccess)
            return Result<CreateRequestResponseDto>.Failure(attachmentResult.Error!);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<CreateRequestResponseDto>.Success(new CreateRequestResponseDto
        {
            Id = broadcastRequest.Id,
            RequestId = broadcastRequest.RequestId,
            Type = "Broadcast",
            Status = broadcastRequest.Status.ToString(),
            CreatedAt = broadcastRequest.CreatedAt,
            LegalServiceId = broadcastRequest.LegalServiceId!.Value,
            ServiceType = broadcastRequest.ServiceType,
            ClientDeadline = broadcastRequest.ClientDeadline,
            PreferredCommunicationMethod = broadcastRequest.PreferredCommunicationMethod,
            Urgency = broadcastRequest.Urgency,
            RequestDetails = detailsResult.Value.Details
        });
    }
}
