using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlMostashar.Application.Features.Lawyers.Queries.GetPrivateDocument;

public sealed class GetLawyerPrivateDocumentQueryHandler
    : IRequestHandler<GetLawyerPrivateDocumentQuery, Result<string>>
{
    private readonly IAppDbContext _db;

    public GetLawyerPrivateDocumentQueryHandler(IAppDbContext db) => _db = db;

    public async Task<Result<string>> Handle(
        GetLawyerPrivateDocumentQuery request,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(request.DocumentType))
        {
            return Result<string>.Failure(
                new Error("Lawyer.DocumentTypeInvalid", "The requested document type is invalid."));
        }

        var lawyer = await _db.Lawyers
            .AsNoTracking()
            .Where(l => l.Id == request.LawyerId)
            .Select(l => new
            {
                l.SSN_Url,
                l.SyndicateCardUrl,
                PendingSSN = l.PendingProfileChange == null ? null : l.PendingProfileChange.SSN_Url,
                PendingSyndicateCard = l.PendingProfileChange == null ? null : l.PendingProfileChange.SyndicateCardUrl
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (lawyer is null)
        {
            return Result<string>.Failure(
                new Error("Lawyer.NotFound", "Lawyer not found."));
        }

        var key = (request.DocumentType, request.UsePendingProfileChange) switch
        {
            (LawyerPrivateDocumentType.Ssn, false) => lawyer.SSN_Url,
            (LawyerPrivateDocumentType.SyndicateCard, false) => lawyer.SyndicateCardUrl,
            (LawyerPrivateDocumentType.Ssn, true) => lawyer.PendingSSN,
            (LawyerPrivateDocumentType.SyndicateCard, true) => lawyer.PendingSyndicateCard,
            _ => null
        };

        if (string.IsNullOrWhiteSpace(key))
        {
            return Result<string>.Failure(
                new Error("Lawyer.DocumentNotFound", "The requested lawyer document was not found."));
        }

        return Result<string>.Success(key);
    }
}
