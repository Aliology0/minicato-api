using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Lawyers.Queries.GetPrivateDocument;

public enum LawyerPrivateDocumentType
{
    Ssn,
    SyndicateCard,
    PracticeCertificates
}

public sealed record GetLawyerPrivateDocumentQuery(
    int LawyerId,
    LawyerPrivateDocumentType DocumentType) : IRequest<Result<string>>;
