using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Documents.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Documents.Commands.GetPresignedUrl;

public sealed class GetPresignedUrlCommandHandler
    : IRequestHandler<GetPresignedUrlCommand, Result<GetPresignedUrlResponseDto>>
{
    private readonly IStorageService _storageService;
    private readonly IDocumentAccessService _documentAccess;

    public GetPresignedUrlCommandHandler(
        IStorageService storageService,
        IDocumentAccessService documentAccess)
    {
        _storageService = storageService;
        _documentAccess = documentAccess;
    }

    public async Task<Result<GetPresignedUrlResponseDto>> Handle(
        GetPresignedUrlCommand request,
        CancellationToken cancellationToken)
    {
        var documentResult = await _documentAccess.GetForReadAsync(request.DocumentId, cancellationToken);
        if (!documentResult.IsSuccess)
            return Result<GetPresignedUrlResponseDto>.Failure(documentResult.Error!);

        var expiration = request.ExpirationMinutes ?? 60;
        var document = documentResult.Value!;
        var url = _storageService.GetPresignedUrl(document.DocumentUrl, expiration);

        return Result<GetPresignedUrlResponseDto>.Success(new GetPresignedUrlResponseDto
        {
            DocumentId = document.Id,
            DocumentName = document.DocumentName,
            Url = url,
            SizeInBytes = document.SizeInBytes,
            Type = document.DocumentType,
            ExpirationMinutes = expiration,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiration)
        });
    }
}
