using AlMostashar.Application.Common.Helpers;
using AlMostashar.Application.Common.Models;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using MediatR;

namespace AlMostashar.Application.Features.LegalService.Queries.GetServiceTypes;

public class GetServiceTypesQueryHandler : IRequestHandler<GetServiceTypesQuery, Result<IEnumerable<EnumDto>>>
{
    public Task<Result<IEnumerable<EnumDto>>> Handle(GetServiceTypesQuery request, CancellationToken cancellationToken)
    {
        var result = EnumHelper.GetEnumList<ServiceType>();
        return Task.FromResult(Result<IEnumerable<EnumDto>>.Success(result));
    }
}
