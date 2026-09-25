using AlMostashar.Application.Features.Cases.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Cases.Queries.GetClientCases;

public record GetClientCasesQuery(
) : IRequest<Result<List<ClientCasesDto>>>;
