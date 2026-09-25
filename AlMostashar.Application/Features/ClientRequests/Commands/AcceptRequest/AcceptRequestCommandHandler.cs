using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.ClientRequests.Commands.AcceptRequest;

public class AcceptRequestCommandHandler : IRequestHandler<AcceptRequestCommand, Result<string>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AcceptRequestCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<string>> Handle(AcceptRequestCommand request, CancellationToken cancellationToken)
    {
        var clientRequest = await _db.ClientRequests
            .Include(r => r.LawyersServices)
            .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken);

        if (clientRequest is null)
            return Result<string>.Failure(
                new Error("Request.NotFound", Messages.Generic.NotFound("Request")));

        if (clientRequest.Status != ClientRequestStatus.Pending)
            return Result<string>.Failure(
                new Error("Request.InvalidStatus", "لا يمكن قبول طلب غير معلق."));

        if (clientRequest is BroadcastRequest)
        {
            return Result<string>.Failure(
                new Error("Request.BroadcastMustUseOffers", "Broadcast requests must be accepted by the client via offers, not directly by lawyers."));
        }

        var currentLawyerId = _currentUser.UserId;

        var (totalAmount, platformFee, lawyerAmount, error) =
            await CalculateAmountsAsync(clientRequest, currentLawyerId, null, cancellationToken);

        if (error is not null)
            return Result<string>.Failure(error);

        var acceptedLegalServiceIdResult = ResolveAcceptedLegalServiceId(clientRequest, null);
        if (!acceptedLegalServiceIdResult.IsSuccess)
            return Result<string>.Failure(acceptedLegalServiceIdResult.Error!);

        clientRequest.MarkAccepted(
            currentLawyerId,
            acceptedLegalServiceIdResult.Value,
            totalAmount,
            platformFee,
            lawyerAmount);

        await _db.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(Messages.GeneralSuccess.RequestAccepted);
    }

    private async Task<(decimal total, decimal platformFee, decimal lawyerAmount, Error? error)>
        CalculateAmountsAsync(ClientRequest clientRequest, int lawyerId, int? legalServiceId, CancellationToken cancellationToken)
    {
        if (clientRequest is DirectRequest)
        {
            if (clientRequest.LawyerServiceLawyerId != lawyerId)
                return (0, 0, 0, new Error("Request.Unauthorized", "هذا الطلب ليس موجهاً إليك."));

            var lawyerService = clientRequest.LawyersServices;

            if (lawyerService is null)
                return (0, 0, 0, new Error("Request.NotFound", Messages.Generic.NotFound("LawyerService")));

            if (lawyerService.Price <= 0)
                return (0, 0, 0, new Error("Request.PriceAgreementRequired", "A payable offer must be agreed before accepting this request."));

            var total = lawyerService.Price;
            var platformFee = Math.Round(total * 0.10m, 2);
            var lawyerAmount = total - platformFee;
            return (total, platformFee, lawyerAmount, null);
        }

        if (clientRequest is BroadcastRequest broadcastRequest)
        {
            if (legalServiceId is null)
                return (0, 0, 0, new Error("Request.MissingService", "يجب تحديد الخدمة القانونية لقبول الطلب."));

            var lawyerService = await _db.LawyerServices
                .FirstOrDefaultAsync(
                    ls => ls.LawyerId == lawyerId
                       && ls.LegalServiceId == legalServiceId
                       && ls.IsActive,
                    cancellationToken);

            if (lawyerService is null)
                return (0, 0, 0, new Error("Request.NotFound", Messages.Generic.NotFound("LawyerService")));

            clientRequest.LawyerServiceLawyerId = lawyerId;
            clientRequest.LawyerServiceLegalServiceId = legalServiceId;

            var total = broadcastRequest.Budget;
            var platformFee = Math.Round(total * 0.10m, 2);
            var lawyerAmount = total - platformFee;
            return (total, platformFee, lawyerAmount, null);
        }

        return (0, 0, 0, new Error("Request.UnsupportedType", "نوع الطلب غير مدعوم للقبول."));
    }

    private static Result<int?> ResolveAcceptedLegalServiceId(ClientRequest clientRequest, int? requestedLegalServiceId)
    {
        if (clientRequest is DirectRequest)
        {
            if (clientRequest.LawyerServiceLegalServiceId is null)
                return Result<int?>.Failure(
                    new Error("Request.InvalidState", "Cannot accept request because legal service is missing."));

            if (requestedLegalServiceId is not null &&
                requestedLegalServiceId != clientRequest.LawyerServiceLegalServiceId)
            {
                return Result<int?>.Failure(
                    new Error("Request.InvalidService", "Cannot change legal service for direct request."));
            }

            return Result<int?>.Success(clientRequest.LawyerServiceLegalServiceId);
        }

        // Broadcast: legalServiceId was already validated in CalculateAmountsAsync.
        return Result<int?>.Success(requestedLegalServiceId);
    }
}
