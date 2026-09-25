using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;

namespace AlMostashar.Application.Common.Interfaces;

public interface IClientRequestCaseMapper
{
    Result<Case> Create(ClientRequest request, int lawyerId, object requestDetails);
}
