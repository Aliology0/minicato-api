using AlMostashar.Application.Features.Escrows.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Escrows.Commands.RefundEscrow;

public record RefundEscrowCommand(int EscrowId) : IRequest<Result<EscrowOperationResponseDto>>;
