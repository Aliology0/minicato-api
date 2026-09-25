using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;

namespace AlMostashar.Application.Common.Interfaces;

public interface IClientRequestAttachmentService
{
    Task<Result<bool>> LinkAsync(ClientRequest request, IReadOnlyCollection<int>? attachmentIds, int clientId, CancellationToken cancellationToken);
}
