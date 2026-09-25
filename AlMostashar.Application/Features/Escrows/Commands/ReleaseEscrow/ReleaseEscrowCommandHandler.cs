using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Escrows.DTOs;
using AlMostashar.Application.Features.Escrows.Services;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Escrows.Commands.ReleaseEscrow;

public class ReleaseEscrowCommandHandler : IRequestHandler<ReleaseEscrowCommand, Result<EscrowOperationResponseDto>>
{
    private readonly IAppDbContext _db;
    private readonly IEscrowSettlementService _settlementService;

    public ReleaseEscrowCommandHandler(IAppDbContext db, IEscrowSettlementService settlementService)
    {
        _db = db;
        _settlementService = settlementService;
    }

    public async Task<Result<EscrowOperationResponseDto>> Handle(
        ReleaseEscrowCommand request,
        CancellationToken cancellationToken)
    {
        Result<EscrowOperationResponseDto>? result = null;

        try
        {
            await _db.ExecuteInTransactionAsync(async ct =>
            {
                var settlement = await _settlementService.ReleaseToLawyerAsync(
                    request.EscrowId,
                    request.AllowDisputedEscrow,
                    ct);

                if (!settlement.IsSuccess)
                {
                    result = Result<EscrowOperationResponseDto>.Failure(settlement.Error!);
                    return;
                }

                result = Result<EscrowOperationResponseDto>.Success(new EscrowOperationResponseDto
                {
                    Message = "Escrow released successfully.",
                    EscrowId = settlement.Value!.EscrowId,
                    RequestId = settlement.Value.RequestId,
                    Amount = settlement.Value.EscrowAmount,
                    ReleasedAmount = settlement.Value.ReleasedAmount,
                    Status = Domain.ValueObject.Enum.EscrowStatus.Released
                });
            }, cancellationToken);
        }
        catch (EscrowReleaseConflictException ex)
        {
            result = Result<EscrowOperationResponseDto>.Failure(
                new Error("Escrow.ReleaseConflict", ex.Message));
        }

        return result!;
    }
}
