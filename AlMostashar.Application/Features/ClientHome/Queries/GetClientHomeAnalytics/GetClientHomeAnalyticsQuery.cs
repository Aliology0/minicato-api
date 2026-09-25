using AlMostashar.Application.Features.ClientHome.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.ClientHome.Queries.GetClientHomeAnalytics;

public class GetClientHomeAnalyticsQuery : IRequest<Result<ClientHomeAnalyticsDto>>
{
}
