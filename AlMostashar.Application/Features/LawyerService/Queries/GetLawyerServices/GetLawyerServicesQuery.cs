using AlMostashar.Application.Features.LawyerService.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.LawyerService.Queries.GetLawyerServices
{
    public class GetLawyerServicesQuery:IRequest<Result<List<LawyerServiceResponse>>>
    {
    }
}
