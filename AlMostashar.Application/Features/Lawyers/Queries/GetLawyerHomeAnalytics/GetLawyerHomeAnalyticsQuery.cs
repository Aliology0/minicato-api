using AlMostashar.Application.Features.Lawyers.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Lawyers.Queries.GetLawyerHomeAnalytics;

public record GetLawyerHomeAnalyticsQuery() : IRequest<Result<LawyerAnalyticsDto>>;
