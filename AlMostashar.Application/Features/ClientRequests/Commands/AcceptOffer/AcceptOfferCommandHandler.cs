using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.ClientRequests.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.ClientRequests.Commands.AcceptOffer;

public class AcceptOfferCommandHandler : IRequestHandler<AcceptOfferCommand, Result<AcceptOfferResponseDto>>
{
    private const string AcceptedOfferUniqueIndexName = "UX_RequestOffers_AcceptedPerRequest";
    private const string SuccessMessage = "Offer accepted successfully.";

    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AcceptOfferCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<AcceptOfferResponseDto>> Handle(AcceptOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await _db.RequestOffers
            .Include(o => o.ClientRequest)
            .FirstOrDefaultAsync(o => o.Id == request.OfferId, cancellationToken);

        if (offer is null)
            return Result<AcceptOfferResponseDto>.Failure(new Error("Offer.NotFound", Messages.Generic.NotFound("Offer")));

        if (offer.ClientRequest.ClientId != _currentUser.UserId)
            return Result<AcceptOfferResponseDto>.Failure(new Error("Offer.Unauthorized", Messages.Offers.NotYourRequest));

        if (offer.Status != OfferStatus.Pending)
            return Result<AcceptOfferResponseDto>.Failure(new Error("Offer.InvalidStatus", Messages.Offers.OnlyPendingCanBeAccepted));

        if (offer.ClientRequest.Status != ClientRequestStatus.Pending)
            return Result<AcceptOfferResponseDto>.Failure(new Error("Offer.RequestNotPending", Messages.Offers.CannotAcceptNonPending));

        var hasAcceptedOffer = await _db.RequestOffers
            .AnyAsync(
                o => o.ClientRequestId == offer.ClientRequestId &&
                     o.Status == OfferStatus.Accepted,
                cancellationToken);

        if (hasAcceptedOffer)
        {
            return Result<AcceptOfferResponseDto>.Failure(
                new Error("Offer.Conflict", "Another offer has already been accepted for this request."));
        }

        var acceptedLegalServiceResult = ResolveAcceptedLegalServiceId(offer.ClientRequest, offer);
        if (!acceptedLegalServiceResult.IsSuccess)
            return Result<AcceptOfferResponseDto>.Failure(acceptedLegalServiceResult.Error!);

        var acceptedLegalServiceId = acceptedLegalServiceResult.Value;

        var lawyerServiceExists = await _db.LawyerServices
            .AnyAsync(
                ls => ls.LawyerId == offer.LawyerId &&
                      ls.LegalServiceId == acceptedLegalServiceId &&
                      ls.IsActive,
                cancellationToken);

        if (!lawyerServiceExists)
            return Result<AcceptOfferResponseDto>.Failure(
                new Error("Offer.InvalidLawyerService", Messages.Generic.NotFound("Active LawyerService")));

        var total = offer.OfferedAmount;
        var platformFee = Math.Round(total * 0.10m, 2);
        var lawyerAmount = total - platformFee;

        offer.Status = OfferStatus.Accepted;
        offer.RespondedAt = DateTime.UtcNow;

        var otherPendingOffers = await _db.RequestOffers
            .Where(o => o.ClientRequestId == offer.ClientRequestId &&
                        o.Id != offer.Id &&
                        o.Status == OfferStatus.Pending)
            .ToListAsync(cancellationToken);

        foreach (var pendingOffer in otherPendingOffers)
        {
            pendingOffer.Status = OfferStatus.Rejected;
            pendingOffer.RespondedAt = DateTime.UtcNow;
        }

        offer.ClientRequest.MarkAccepted(
            lawyerId: offer.LawyerId,
            legalServiceId: acceptedLegalServiceId,
            totalAmount: total,
            platformFee: platformFee,
            lawyerAmount: lawyerAmount);
        offer.ClientRequest.MarkAcceptedOffer(offer.Id);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsAcceptedOfferUniqueConflict(ex))
        {
            return Result<AcceptOfferResponseDto>.Failure(
                new Error("Offer.Conflict", "Another offer has already been accepted for this request."));
        }

        var invoiceId = await _db.Invoices
            .Where(i => i.ClientRequestId == offer.ClientRequestId)
            .Select(i => (int?)i.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (!invoiceId.HasValue)
        {
            throw new InvalidOperationException(
                $"Invoice was not created for client request {offer.ClientRequestId} after accepting offer {offer.Id}.");
        }

        return Result<AcceptOfferResponseDto>.Success(new AcceptOfferResponseDto
        {
            Message = SuccessMessage,
            InvoiceId = invoiceId.Value,
            ClientRequestId = offer.ClientRequestId
        });
    }

    private static bool IsAcceptedOfferUniqueConflict(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message ?? exception.Message;

        return message.Contains(AcceptedOfferUniqueIndexName, StringComparison.OrdinalIgnoreCase) ||
               message.Contains("UNIQUE constraint failed: RequestOffers.ClientRequestId", StringComparison.OrdinalIgnoreCase);
    }

    private static Result<int> ResolveAcceptedLegalServiceId(ClientRequest clientRequest, RequestOffer offer)
    {
        if (clientRequest is DirectRequest)
        {
            if (clientRequest.LawyerServiceLawyerId != offer.LawyerId)
            {
                return Result<int>.Failure(
                    new Error("Offer.InvalidLawyer", "Offer lawyer does not match the assigned lawyer for this direct request."));
            }

            if (!clientRequest.LawyerServiceLegalServiceId.HasValue)
            {
                return Result<int>.Failure(
                    new Error("Offer.InvalidRequestState", "Direct request has no configured legal service."));
            }

            var fixedLegalServiceId = clientRequest.LawyerServiceLegalServiceId.Value;
            if (offer.LegalServiceId != fixedLegalServiceId)
            {
                return Result<int>.Failure(
                    new Error("Offer.InvalidService", "Offer legal service does not match the direct request legal service."));
            }

            return Result<int>.Success(fixedLegalServiceId);
        }

        return Result<int>.Success(offer.LegalServiceId);
    }
}
