using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.ClientRequests.DTOs;
using AlMostashar.Domain.ValueObject.Enum;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.ClientRequests.Queries;

internal static class RequestQueryDataLoader
{
    public static async Task<Dictionary<int, List<DocumentDto>>> LoadDocumentsByRequestIdsAsync(
        IAppDbContext db,
        IReadOnlyCollection<int> requestIds,
        CancellationToken cancellationToken)
    {
        if (requestIds.Count == 0)
            return new Dictionary<int, List<DocumentDto>>();

        var documentRows = await db.CaseDocuments
            .AsNoTracking()
            .Where(d => d.ClientRequestId.HasValue && requestIds.Contains(d.ClientRequestId.Value))
            .OrderBy(d => d.CreatedAt)
            .Select(d => new
            {
                RequestId = d.ClientRequestId!.Value,
                Document = new DocumentDto
                {
                    Id = d.Id,
                    DocumentName = d.DocumentName,
                    Type = d.DocumentType,
                    SizeInBytes = d.SizeInBytes,
                    CreatedAt = d.CreatedAt

                }
            })
            .ToListAsync(cancellationToken);

        return documentRows
            .GroupBy(x => x.RequestId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Document).ToList());
    }

    public static async Task<Dictionary<int, int>> LoadOfferCountsByRequestIdsAsync(
        IAppDbContext db,
        IReadOnlyCollection<int> requestIds,
        CancellationToken cancellationToken)
    {
        if (requestIds.Count == 0)
            return new Dictionary<int, int>();

        return await db.RequestOffers
            .AsNoTracking()
            .Where(o => requestIds.Contains(o.ClientRequestId) &&
                        o.Status == OfferStatus.Pending)
            .GroupBy(o => o.ClientRequestId)
            .Select(g => new { RequestId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RequestId, x => x.Count, cancellationToken);
    }
}
