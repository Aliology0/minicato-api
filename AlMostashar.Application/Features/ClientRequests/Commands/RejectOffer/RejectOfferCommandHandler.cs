using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.ClientRequests.Commands.RejectOffer;

public class RejectOfferCommandHandler : IRequestHandler<RejectOfferCommand, Result<string>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RejectOfferCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<string>> Handle(RejectOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await _db.RequestOffers
            .Include(o => o.ClientRequest)
            .FirstOrDefaultAsync(o => o.Id == request.OfferId, cancellationToken);

        if (offer is null)
            return Result<string>.Failure(new Error("Offer.NotFound", Messages.Generic.NotFound("Offer")));

        if (offer.ClientRequest.ClientId != _currentUser.UserId)
            return Result<string>.Failure(new Error("Offer.Unauthorized", Messages.Offers.NotYourRequest));

        if (offer.Status != OfferStatus.Pending)
            return Result<string>.Failure(new Error("Offer.InvalidStatus", Messages.Offers.OnlyPendingCanBeRejected));

        if (offer.ClientRequest.Status != ClientRequestStatus.Pending)
            return Result<string>.Failure(new Error("Offer.RequestNotPending", Messages.Offers.CannotRejectNonPending));

        offer.Status = OfferStatus.Rejected;
        offer.RespondedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(Messages.GeneralSuccess.OfferRejected);
    }
}
