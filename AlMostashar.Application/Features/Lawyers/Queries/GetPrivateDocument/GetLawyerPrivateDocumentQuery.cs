using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Lawyers.Queries.GetPrivateDocument;

public enum LawyerPrivateDocumentType
{
    Ssn,
    SyndicateCard
}

public sealed record GetLawyerPrivateDocumentQuery(
    int LawyerId,
    LawyerPrivateDocumentType DocumentType,
    bool UsePendingProfileChange = false) : IRequest<Result<string>>;
