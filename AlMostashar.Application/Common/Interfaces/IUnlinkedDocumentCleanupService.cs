namespace AlMostashar.Application.Common.Interfaces;

public interface IUnlinkedDocumentCleanupService
{
    Task<int> CleanupAsync(DateTime olderThanUtc, CancellationToken cancellationToken);
}
