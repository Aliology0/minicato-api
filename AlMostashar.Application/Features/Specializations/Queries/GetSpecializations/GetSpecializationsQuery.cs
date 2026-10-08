using AlMostashar.Application.Features.Specializations.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Specializations.Queries.GetSpecializations;

public record GetSpecializationsQuery : IRequest<Result<IReadOnlyList<SpecializationLookupDto>>>;
