using AlMostashar.Application.Common.Models;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.LegalService.Queries.GetServiceTypes;

public record GetServiceTypesQuery : IRequest<Result<IEnumerable<EnumDto>>>;
