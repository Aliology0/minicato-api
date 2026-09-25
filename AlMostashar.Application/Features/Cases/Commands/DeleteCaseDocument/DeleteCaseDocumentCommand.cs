using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Cases.Commands.DeleteCaseDocument;

public record DeleteCaseDocumentCommand(
    int CaseId,
    int DocumentId
) : IRequest<Result<string>>;
