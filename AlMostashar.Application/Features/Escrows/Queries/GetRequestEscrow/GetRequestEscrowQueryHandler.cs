using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Escrows.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Escrows.Queries.GetRequestEscrow;

public class GetRequestEscrowQueryHandler : IRequestHandler<GetRequestEscrowQuery, Result<EscrowDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetRequestEscrowQueryHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<EscrowDto>> Handle(GetRequestEscrowQuery request, CancellationToken cancellationToken)
    {
        var clientRequest = await _db.ClientRequests
            .AsNoTracking()
            .Where(r => r.Id == request.RequestId)
            .Select(r => new
            {
                r.Id,
                r.ClientId,
                r.LawyerServiceLawyerId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (clientRequest is null)
            return Result<EscrowDto>.Failure(new Error("Request.NotFound", "Request was not found."));

        var currentUserId = _currentUser.UserId;

        var currentClientId = await _db.Clients
            .AsNoTracking()
            .Where(c => c.Id == currentUserId)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var currentLawyerId = await _db.Lawyers
            .AsNoTracking()
            .Where(l => l.Id == currentUserId)
            .Select(l => (int?)l.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var isAdmin = await _db.Admins
            .AsNoTracking()
            .AnyAsync(a => a.Id == currentUserId, cancellationToken);

        var isParticipant = currentClientId == clientRequest.ClientId ||
                            currentLawyerId == clientRequest.LawyerServiceLawyerId;

        if (!isParticipant && !isAdmin)
            return Result<EscrowDto>.Failure(new Error("Escrow.Auth.Unauthorized", "You are not allowed to view this escrow."));

        var escrow = await _db.Escrows
            .AsNoTracking()
            .Where(e => e.RequestId == request.RequestId)
            .Select(e => new EscrowDto
            {
                EscrowId = e.Id,
                RequestId = e.RequestId,
                Amount = e.Amount,
                Status = e.Status,
                EscrowFundedAt = e.EscrowFundedAt,
                EscrowReleasedAt = e.EscrowReleasedAt,
                EscrowRefundedAt = e.EscrowRefundedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (escrow is null)
            return Result<EscrowDto>.Failure(new Error("Escrow.NotFound", "Escrow was not found."));

        return Result<EscrowDto>.Success(escrow);
    }
}
