using AlMostashar.Application.Features.Cases.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Cases.Commands.UploadCaseDocument;

public record UploadCaseDocumentCommand(
    int CaseId,
    int DocumentId
) : IRequest<Result<CaseDocumentDto>>;
