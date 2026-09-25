using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Escrows.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Escrows.Commands.MarkEscrowAsDisputed;

public class MarkEscrowAsDisputedCommandHandler : IRequestHandler<MarkEscrowAsDisputedCommand, Result<EscrowOperationResponseDto>>
{
    private readonly IAppDbContext _db;

    public MarkEscrowAsDisputedCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<EscrowOperationResponseDto>> Handle(
        MarkEscrowAsDisputedCommand request,
        CancellationToken cancellationToken)
    {
        Result<EscrowOperationResponseDto>? result = null;

        await _db.ExecuteInTransactionAsync(async ct =>
        {
            var escrow = await _db.Escrows
                .FirstOrDefaultAsync(e => e.Id == request.EscrowId, ct);

            if (escrow is null)
            {
                result = Failure("Escrow.NotFound", "Escrow was not found.");
                return;
            }

            if (escrow.Status != EscrowStatus.Funded)
            {
                result = Failure("Escrow.InvalidStatus", $"Only funded escrows can be marked as disputed. Current status is {escrow.Status}.");
                return;
            }

            escrow.Status = EscrowStatus.Disputed;
            escrow.EscrowDisputedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            result = Success(escrow, "Escrow marked as disputed successfully.");
        }, cancellationToken);

        return result!;
    }

    private static Result<EscrowOperationResponseDto> Success(Escrow escrow, string message)
    {
        return Result<EscrowOperationResponseDto>.Success(new EscrowOperationResponseDto
        {
            Message = message,
            EscrowId = escrow.Id,
            RequestId = escrow.RequestId,
            Amount = escrow.Amount,
            Status = escrow.Status
        });
    }

    private static Result<EscrowOperationResponseDto> Failure(string code, string message)
    {
        return Result<EscrowOperationResponseDto>.Failure(new Error(code, message));
    }
}
