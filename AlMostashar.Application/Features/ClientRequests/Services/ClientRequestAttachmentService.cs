using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.ClientRequests.Services;

public sealed class ClientRequestAttachmentService : IClientRequestAttachmentService
{
    private readonly IAppDbContext _db;

    public ClientRequestAttachmentService(IAppDbContext db) => _db = db;

    public async Task<Result<bool>> LinkAsync(
        ClientRequest request,
        IReadOnlyCollection<int>? attachmentIds,
        int clientId,
        CancellationToken cancellationToken)
    {
        if (attachmentIds is null || attachmentIds.Count == 0)
            return Result<bool>.Success(true);

        var ids = attachmentIds.Distinct().ToArray();
        var documents = await _db.CaseDocuments
            .Where(d => ids.Contains(d.Id) &&
                        d.UploadedByUserId == clientId &&
                        d.CleanupClaimToken == null &&
                        d.ClientRequestId == null &&
                        d.CaseId == null &&
                        d.ReportId == null)
            .ToListAsync(cancellationToken);

        if (documents.Count != ids.Length)
            return Result<bool>.Failure(new Error(
                "Request.InvalidAttachments",
                "One or more attachments do not exist, are already linked, or do not belong to the current client."));

        foreach (var document in documents)
            document.ClientRequest = request;

        return Result<bool>.Success(true);
    }
}
