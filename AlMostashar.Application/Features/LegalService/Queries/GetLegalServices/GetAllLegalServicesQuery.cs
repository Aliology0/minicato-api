using AlMostashar.Application.Features.LegalService.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.LegalService.Queries.GetLegalServices
{
    public class GetLegalServicesQuery : IRequest<Result<List<LegalServiceResponse>>>
    {
    }
}
