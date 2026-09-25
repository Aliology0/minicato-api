using AlMostashar.Application.Features.Escrows.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Escrows.Queries.GetRequestEscrow;

public record GetRequestEscrowQuery(int RequestId) : IRequest<Result<EscrowDto>>;
