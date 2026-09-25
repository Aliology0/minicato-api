using AlMostashar.Application.Features.Lawyers.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Lawyers.Queries.GetLawyerProfile;

public record GetLawyerProfileQuery(int LawyerId) : IRequest<Result<LawyerProfileDto>>;
