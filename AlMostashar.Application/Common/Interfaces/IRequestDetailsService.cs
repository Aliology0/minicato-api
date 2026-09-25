using AlMostashar.Application.Features.ClientRequests.RequestDetails;
using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;
using System.Text.Json;
using AlMostashar.Domain.Entities;

namespace AlMostashar.Application.Common.Interfaces;

public interface IRequestDetailsService
{
    Result<ValidatedRequestDetails> ValidateAndNormalize(ServiceType serviceType, JsonElement requestDetails);
    void AttachToRequest(ClientRequest request, object details);
    object? MapFromEntity(ClientRequest request);
}
