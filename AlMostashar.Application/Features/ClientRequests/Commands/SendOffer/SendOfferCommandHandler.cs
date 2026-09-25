using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.ClientRequests.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.ClientRequests.Commands.SendOffer;

public class SendOfferCommandHandler : IRequestHandler<SendOfferCommand, Result<RequestOfferDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public SendOfferCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<RequestOfferDto>> Handle(SendOfferCommand request, CancellationToken cancellationToken)
    {
        var lawyerId = _currentUser.UserId;

        var lawyer = await _db.Lawyers
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == lawyerId, cancellationToken);

        if (lawyer is null)
            return Result<RequestOfferDto>.Failure(new Error("Auth.Forbidden", Messages.Auth.OnlyLawyersCanSendOffers));

        if (lawyer.AccountStatus != AccountStatus.Active)
            return Result<RequestOfferDto>.Failure(new Error("Lawyer.NotVerified", Messages.Generic.NotVerified("Lawyer")));

        var clientRequest = await _db.ClientRequests
            .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken);

        if (clientRequest is null)
            return Result<RequestOfferDto>.Failure(new Error("Request.NotFound", Messages.Generic.NotFound("Request")));

        if (clientRequest.Status != ClientRequestStatus.Pending)
            return Result<RequestOfferDto>.Failure(new Error("Offer.RequestNotPending", Messages.Offers.CannotSendNonPending));

        var resolvedLegalServiceResult = ResolveLegalServiceIdForOffer(clientRequest, lawyerId, request.LegalServiceId);
        if (!resolvedLegalServiceResult.IsSuccess)
            return Result<RequestOfferDto>.Failure(resolvedLegalServiceResult.Error!);

        var resolvedLegalServiceId = resolvedLegalServiceResult.Value;

        var lawyerService = await _db.LawyerServices
            .Include(ls => ls.LegalService)
            .FirstOrDefaultAsync(
                ls => ls.LawyerId == lawyerId &&
                      ls.LegalServiceId == resolvedLegalServiceId &&
                      ls.IsActive,
                cancellationToken);

        if (lawyerService is null)
            return Result<RequestOfferDto>.Failure(
                new Error("Offer.InvalidLawyerService", Messages.Generic.NotFound("Active LawyerService")));

        var exists = await _db.RequestOffers
            .AnyAsync(
                o => o.ClientRequestId == request.RequestId &&
                     o.LawyerId == lawyerId &&
                     o.Status == OfferStatus.Pending,
                cancellationToken);

        if (exists)
            return Result<RequestOfferDto>.Failure(new Error("Offer.Conflict", Messages.Offers.PendingOfferExists));

        var offer = new RequestOffer
        {
            ClientRequestId = request.RequestId,
            LawyerId = lawyerId,
            LegalServiceId = resolvedLegalServiceId,
            OfferedAmount = request.OfferedAmount,
            Note = request.Note,
            Status = OfferStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _db.RequestOffers.Add(offer);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<RequestOfferDto>.Success(new RequestOfferDto
        {
            OfferId = offer.Id,
            RequestId = clientRequest.Id,
            RequestTitle = clientRequest.Title,
            LawyerId = lawyerId,
            LawyerName = lawyer.FullName,
            LawyerProfileImage = lawyer.AvatarUrl,
            LegalServiceId = lawyerService.LegalServiceId,
            LegalServiceTitle = lawyerService.LegalService.Title,
            OfferedAmount = offer.OfferedAmount,
            Note = offer.Note,
            Status = offer.Status,
            CreatedAt = offer.CreatedAt,
            RespondedAt = offer.RespondedAt
        });
    }

    private static Result<int> ResolveLegalServiceIdForOffer(
        ClientRequest clientRequest,
        int currentLawyerId,
        int? requestedLegalServiceId)
    {
        if (clientRequest is BroadcastRequest)
        {
            if (requestedLegalServiceId is null)
            {
                return Result<int>.Failure(
                    new Error("Offer.MissingService", "LegalServiceId is required for broadcast offers."));
            }

            if (!clientRequest.LawyerServiceLegalServiceId.HasValue || clientRequest.LawyerServiceLegalServiceId.Value <= 0)
            {
                return Result<int>.Failure(
                    new Error("Offer.InvalidState", "Broadcast request has no configured legal service."));
            }

            if (requestedLegalServiceId.Value != clientRequest.LawyerServiceLegalServiceId.Value)
            {
                return Result<int>.Failure(
                    new Error("Offer.InvalidService", "Offer legal service must match the broadcast request service."));
            }

            return Result<int>.Success(clientRequest.LawyerServiceLegalServiceId.Value);
        }

        if (clientRequest is DirectRequest)
        {
            if (clientRequest.LawyerServiceLawyerId != currentLawyerId)
            {
                return Result<int>.Failure(
                    new Error("Offer.Unauthorized", "Only the assigned lawyer can submit an offer for this direct request."));
            }

            if (!clientRequest.LawyerServiceLegalServiceId.HasValue)
            {
                return Result<int>.Failure(
                    new Error("Offer.InvalidState", "Direct request has no configured legal service."));
            }

            var fixedLegalServiceId = clientRequest.LawyerServiceLegalServiceId.Value;
            if (requestedLegalServiceId.HasValue && requestedLegalServiceId.Value != fixedLegalServiceId)
            {
                return Result<int>.Failure(
                    new Error("Offer.InvalidService", "Cannot change legal service for direct request offers."));
            }

            return Result<int>.Success(fixedLegalServiceId);
        }

        return Result<int>.Failure(new Error("Offer.UnsupportedType", Messages.Offers.UnsupportedRequestType));
    }
}
