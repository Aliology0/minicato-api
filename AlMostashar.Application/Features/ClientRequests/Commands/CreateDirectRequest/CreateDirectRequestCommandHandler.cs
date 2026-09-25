using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Locations;
using AlMostashar.Application.Features.ClientRequests.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.ClientRequests.Commands.CreateDirectRequest;

public class CreateDirectRequestCommandHandler
    : IRequestHandler<CreateDirectRequestCommand, Result<CreateRequestResponseDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILocationCatalog _locationCatalog;
    private readonly IRequestDetailsService _requestDetailsService;
    private readonly IClientRequestAttachmentService _attachmentService;
    private readonly INotificationService _notificationService;

    public CreateDirectRequestCommandHandler(
        IAppDbContext db,
        ICurrentUserService currentUser,
        ILocationCatalog locationCatalog,
        IRequestDetailsService requestDetailsService,
        IClientRequestAttachmentService attachmentService,
        INotificationService notificationService)
    {
        _db = db;
        _currentUser = currentUser;
        _locationCatalog = locationCatalog;
        _requestDetailsService = requestDetailsService;
        _attachmentService = attachmentService;
        _notificationService = notificationService;
    }

    public async Task<Result<CreateRequestResponseDto>> Handle(
        CreateDirectRequestCommand request, CancellationToken cancellationToken)
    {
        var locationResult = RequestLocationResolver.ResolveRequired(
            _locationCatalog,
            request.GovernorateId,
            request.CityId);

        if (!locationResult.IsSuccess)
            return Result<CreateRequestResponseDto>.Failure(locationResult.Error!);
        var location = locationResult.Value!;

        // 1. Validate LawyerService exists
        var lawyerService = await _db.LawyerServices
            .Include(ls => ls.Lawyer)
            .Include(ls => ls.LegalService)
            .FirstOrDefaultAsync(
                ls => ls.LawyerId == request.LawyerId
                   && ls.LegalServiceId == request.LegalServiceId
                   && ls.IsActive,
                cancellationToken);

        if (lawyerService is null)
            return Result<CreateRequestResponseDto>.Failure(
                new Error("Request.NotFound", Messages.Generic.NotFound("LawyerService")));

        if (!lawyerService.LegalService.IsActive)
            return Result<CreateRequestResponseDto>.Failure(
                new Error("Request.InvalidLegalService", Messages.Generic.NotFound("Active LegalService")));

        // 2. Validate lawyer is active
        if (lawyerService.Lawyer.AccountStatus != AccountStatus.Active)
            return Result<CreateRequestResponseDto>.Failure(
                new Error("Request.LawyerNotVerified", Messages.Generic.NotVerified("Lawyer")));

        var detailsResult = _requestDetailsService.ValidateAndNormalize(
            lawyerService.LegalService.ServiceType, request.RequestDetails);
        if (!detailsResult.IsSuccess)
            return Result<CreateRequestResponseDto>.Failure(detailsResult.Error!);

        // 3. Create DirectRequest (no invoice yet — created when lawyer accepts)
        var directRequest = new DirectRequest
        {
            RequestId = $"REQ-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}",
            Title = request.Title,
            ProblemDetails = request.ProblemDetails,
            GovernorateId = location.GovernorateId,
            Governorate = location.Governorate,
            CityId = location.CityId,
            City = location.City,
            ClientId = _currentUser.UserId,
            LegalServiceId = lawyerService.LegalServiceId,
            ServiceType = lawyerService.LegalService.ServiceType,
            ClientDeadline = request.ClientDeadline,
            PreferredCommunicationMethod = request.PreferredCommunicationMethod,
            Urgency = request.Urgency,
            LawyerServiceLawyerId = request.LawyerId,
            LawyerServiceLegalServiceId = request.LegalServiceId,
            Status = ClientRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        _requestDetailsService.AttachToRequest(directRequest, detailsResult.Value!.Details);

        _db.DirectRequests.Add(directRequest);
        var attachmentResult = await _attachmentService.LinkAsync(
            directRequest, request.AttachmentIds, _currentUser.UserId, cancellationToken);
        if (!attachmentResult.IsSuccess)
            return Result<CreateRequestResponseDto>.Failure(attachmentResult.Error!);

        //Add Notification
        var title = Messages.Notifications.NewDirectRequestTitle;
        var body = Messages.Notifications.NewDirectRequestBody;

        var notificationEntity = new Notification
        {
            UserId = request.LawyerId,
            Title = title,
            Description = body,
            Type = NotificationType.RequestUpdate,
            SourceId = directRequest.Id,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };
        _db.Notifications.Add(notificationEntity);
        await _db.SaveChangesAsync(cancellationToken);

        await _notificationService.SendToUserAsync(
            request.LawyerId,
            title,
            body,
            NotificationType.RequestUpdate,
            directRequest.Id);


        return Result<CreateRequestResponseDto>.Success(new CreateRequestResponseDto
        {
            Id = directRequest.Id,
            RequestId = directRequest.RequestId,
            Type = "Direct",
            Status = directRequest.Status.ToString(),
            CreatedAt = directRequest.CreatedAt,
            LegalServiceId = directRequest.LegalServiceId!.Value,
            ServiceType = directRequest.ServiceType,
            ClientDeadline = directRequest.ClientDeadline,
            PreferredCommunicationMethod = directRequest.PreferredCommunicationMethod,
            Urgency = directRequest.Urgency,
            RequestDetails = detailsResult.Value.Details
        });
    }
}
