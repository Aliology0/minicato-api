using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;

namespace AlMostashar.Application.Common.Interfaces;

public interface IDocumentAccessService
{
    Task<Result<CaseDocuments>> GetForReadAsync(int documentId, CancellationToken cancellationToken);
    Task<Result<CaseDocuments>> GetUnlinkedForDeleteAsync(int documentId, CancellationToken cancellationToken);
}
