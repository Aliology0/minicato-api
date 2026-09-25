using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Documents.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Documents.Commands.DeleteDocument;

public sealed class DeleteDocumentCommandHandler
    : IRequestHandler<DeleteDocumentCommand, Result<DeleteDocumentResponseDto>>
{
    private readonly IStorageService _storageService;
    private readonly IAppDbContext _db;
    private readonly IDocumentAccessService _documentAccess;

    public DeleteDocumentCommandHandler(
        IStorageService storageService,
        IAppDbContext db,
        IDocumentAccessService documentAccess)
    {
        _storageService = storageService;
        _db = db;
        _documentAccess = documentAccess;
    }

    public async Task<Result<DeleteDocumentResponseDto>> Handle(
        DeleteDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var documentResult = await _documentAccess.GetUnlinkedForDeleteAsync(request.DocumentId, cancellationToken);
        if (!documentResult.IsSuccess)
            return Result<DeleteDocumentResponseDto>.Failure(documentResult.Error!);

        var document = documentResult.Value!;
        var (success, message) = await _storageService.DeleteFileAsync(document.DocumentUrl);
        if (!success)
            return Result<DeleteDocumentResponseDto>.Failure(new Error("Document.DeleteFailed", message));

        _db.CaseDocuments.Remove(document);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<DeleteDocumentResponseDto>.Success(new DeleteDocumentResponseDto
        {
            DocumentId = document.Id,
            Deleted = true,
            Message = message
        });
    }
}
