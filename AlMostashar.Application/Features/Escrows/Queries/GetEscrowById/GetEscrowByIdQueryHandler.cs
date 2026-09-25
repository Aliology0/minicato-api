using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Escrows.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Escrows.Queries.GetEscrowById;

public class GetEscrowByIdQueryHandler : IRequestHandler<GetEscrowByIdQuery, Result<EscrowDto>>
{
    private readonly IAppDbContext _db;

    public GetEscrowByIdQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<EscrowDto>> Handle(GetEscrowByIdQuery request, CancellationToken cancellationToken)
    {
        var escrow = await _db.Escrows
            .AsNoTracking()
            .Where(e => e.Id == request.EscrowId)
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
