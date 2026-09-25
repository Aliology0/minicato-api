using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.ClientRequests.Commands.RejectRequest;

public class RejectRequestCommandHandler : IRequestHandler<RejectRequestCommand, Result<string>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RejectRequestCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<string>> Handle(RejectRequestCommand request, CancellationToken cancellationToken)
    {
        var clientRequest = await _db.ClientRequests
            .Include(r => r.Invoice)
            .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken);

        if (clientRequest is null)
            return Result<string>.Failure(
                new Error("Request.NotFound", Messages.Generic.NotFound("Request")));

        if (clientRequest.Status != ClientRequestStatus.Pending)
            return Result<string>.Failure(
                new Error("Request.InvalidStatus", "لا يمكن رفض طلب غير معلق."));

        var currentLawyerId = _currentUser.UserId;

        // Direct: only the assigned lawyer can reject
        if (clientRequest.LawyerServiceLawyerId is not null
            && clientRequest.LawyerServiceLawyerId != currentLawyerId)
            return Result<string>.Failure(
                new Error("Request.Unauthorized", "هذا الطلب ليس موجهًا إليك."));

        clientRequest.Status = ClientRequestStatus.Rejected;

        // Cancel invoice if exists
        if (clientRequest.Invoice is not null)
            clientRequest.Invoice.Status = InvoiceStatus.Cancelled;

        await _db.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(Messages.GeneralSuccess.RequestRejected);
    }
}
