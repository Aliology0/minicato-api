using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Escrows.DTOs;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Escrows.Commands.RefundEscrow;

public class RefundEscrowCommandHandler : IRequestHandler<RefundEscrowCommand, Result<EscrowOperationResponseDto>>
{
    private readonly IAppDbContext _db;

    public RefundEscrowCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<EscrowOperationResponseDto>> Handle(
        RefundEscrowCommand request,
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

            if (escrow.Status is not (EscrowStatus.Funded or EscrowStatus.Disputed))
            {
                result = Failure("Escrow.InvalidStatus", $"Only funded or disputed escrows can be refunded. Current status is {escrow.Status}.");
                return;
            }

            // Paymob/provider refund execution is owned by the payment module.
            // This command only records the approved escrow refund state.
            escrow.Status = EscrowStatus.Refunded;
            escrow.EscrowRefundedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            result = Success(escrow, "Escrow refunded successfully.");
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
