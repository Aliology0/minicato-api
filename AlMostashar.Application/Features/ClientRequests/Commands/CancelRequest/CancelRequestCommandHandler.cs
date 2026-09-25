using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.ClientRequests.Commands.CancelRequest;

public class CancelRequestCommandHandler : IRequestHandler<CancelRequestCommand, Result<string>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CancelRequestCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<string>> Handle(CancelRequestCommand request, CancellationToken cancellationToken)
    {
        var clientRequest = await _db.ClientRequests
            .Include(r => r.Invoice)
            .FirstOrDefaultAsync(
                r => r.Id == request.RequestId && r.ClientId == _currentUser.UserId,
                cancellationToken);

        if (clientRequest is null)
            return Result<string>.Failure(
                new Error("Request.NotFound", Messages.Generic.NotFound("Request")));

        if (clientRequest.Status != ClientRequestStatus.Pending)
            return Result<string>.Failure(
                new Error("Request.InvalidStatus", "لا يمكن إلغاء طلب غير معلق."));

        clientRequest.Status = ClientRequestStatus.Cancelled;

        if (clientRequest.Invoice is not null)
            clientRequest.Invoice.Status = InvoiceStatus.Cancelled;

        await _db.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(Messages.GeneralSuccess.RequestCanceled);
    }
}
