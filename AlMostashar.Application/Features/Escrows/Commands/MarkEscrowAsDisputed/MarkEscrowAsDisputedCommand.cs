using AlMostashar.Application.Features.Escrows.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Escrows.Commands.MarkEscrowAsDisputed;

public record MarkEscrowAsDisputedCommand(int EscrowId) : IRequest<Result<EscrowOperationResponseDto>>;
