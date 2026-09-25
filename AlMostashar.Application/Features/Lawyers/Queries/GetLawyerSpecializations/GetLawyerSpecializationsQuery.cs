using AlMostashar.Application.Features.Lawyers.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Lawyers.Queries.GetLawyerSpecializations;

public record GetLawyerSpecializationsQuery : IRequest<Result<IEnumerable<LawyerSpecializationLookupDto>>>;
