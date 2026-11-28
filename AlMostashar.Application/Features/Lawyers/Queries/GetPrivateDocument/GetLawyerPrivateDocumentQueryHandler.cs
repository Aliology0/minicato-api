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
                l.PracticeCertificatesUrl
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (lawyer is null)
        {
            return Result<string>.Failure(
                new Error("Lawyer.NotFound", "Lawyer not found."));
        }

        var key = request.DocumentType switch
        {
            LawyerPrivateDocumentType.Ssn => lawyer.SSN_Url,
            LawyerPrivateDocumentType.SyndicateCard => lawyer.SyndicateCardUrl,
            LawyerPrivateDocumentType.PracticeCertificates => lawyer.PracticeCertificatesUrl,
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
