using AlMostashar.Application.Features.Lookups.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Lookups.Queries.GetGovernorates;

public record GetGovernoratesQuery : IRequest<Result<IReadOnlyList<GovernorateLookupDto>>>;
