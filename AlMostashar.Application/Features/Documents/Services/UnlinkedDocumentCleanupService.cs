using AlMostashar.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Documents.Services;

public sealed class UnlinkedDocumentCleanupService : IUnlinkedDocumentCleanupService
{
    private readonly IAppDbContext _db;
    private readonly IStorageService _storage;
    public UnlinkedDocumentCleanupService(IAppDbContext db, IStorageService storage) { _db = db; _storage = storage; }

    public async Task<int> CleanupAsync(DateTime olderThanUtc, CancellationToken cancellationToken)
    {
        var candidateIds = await _db.CaseDocuments.AsNoTracking()
            .Where(x => x.ClientRequestId == null && x.CaseId == null && x.ReportId == null && x.CleanupClaimToken == null && x.CreatedAt < olderThanUtc)
            .OrderBy(x => x.Id).Select(x => x.Id).Take(100).ToListAsync(cancellationToken);
        var deleted = 0;
        foreach (var id in candidateIds)
        {
            var token = Guid.NewGuid().ToString("D");
            var claimed = await _db.TryClaimUnlinkedDocumentAsync(id, olderThanUtc, token, DateTime.UtcNow, cancellationToken);
            if (!claimed) continue;

            var document = await _db.CaseDocuments.SingleAsync(x => x.Id == id && x.CleanupClaimToken == token, cancellationToken);
            var storageResult = await _storage.DeleteFileAsync(document.DocumentUrl);
            if (!storageResult.Success)
            {
                document.CleanupClaimToken = null; document.CleanupClaimedAt = null;
                await _db.SaveChangesAsync(cancellationToken); continue;
            }
            _db.CaseDocuments.Remove(document);
            await _db.SaveChangesAsync(cancellationToken); deleted++;
        }
        return deleted;
    }
}
