using AlMostashar.Application.Features.Escrows.DTOs;
using AlMostashar.Domain.Shared;

namespace AlMostashar.Application.Common.Interfaces;

public interface IEscrowSettlementService
{
    Task<Result<EscrowSettlementResultDto>> ReleaseToLawyerAsync(
        int escrowId,
        bool allowDisputedEscrow,
        CancellationToken cancellationToken);
}
