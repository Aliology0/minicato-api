using AlMostashar.Application.Features.LawyerHome.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.LawyerHome.Queries.GetLawyerActiveCasesSummary;

public class GetLawyerActiveCasesSummaryQuery : IRequest<Result<List<LawyerHomeActiveCaseDto>>>
{
    public int Limit { get; set; } = 5;
}
