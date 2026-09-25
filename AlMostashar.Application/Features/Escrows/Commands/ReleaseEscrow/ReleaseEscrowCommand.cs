using AlMostashar.Application.Features.Escrows.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Escrows.Commands.ReleaseEscrow;

public record ReleaseEscrowCommand(int EscrowId, bool AllowDisputedEscrow = false) : IRequest<Result<EscrowOperationResponseDto>>;
