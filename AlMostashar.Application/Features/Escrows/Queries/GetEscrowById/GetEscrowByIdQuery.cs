using AlMostashar.Application.Features.Escrows.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Escrows.Queries.GetEscrowById;

public record GetEscrowByIdQuery(int EscrowId) : IRequest<Result<EscrowDto>>;
