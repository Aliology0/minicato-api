using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Features.Documents.DTOs;
using AlMostashar.Domain.Shared;
using MediatR;
using static AlMostashar.Application.Helpers.UploadToStorage;
using AlMostashar.Domain.Entities;


namespace AlMostashar.Application.Features.Documents.Commands.UploadDocument
{
    public class UploadDocumentCommandHandler
        : IRequestHandler<UploadDocumentCommand, Result<UploadDocumentResponseDto>>
    {
        private readonly IStorageService _storageService;
        private readonly IAppDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public UploadDocumentCommandHandler(
            IStorageService storageService,
            IAppDbContext db,
            ICurrentUserService currentUser)
        {
            _storageService = storageService;
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<Result<UploadDocumentResponseDto>> Handle(
            UploadDocumentCommand request,
            CancellationToken cancellationToken)
        {
            var fileKey = await UploadAsync(request.File, _storageService);
            var document = new CaseDocuments
            {

                DocumentName = Path.GetFileName(request.File.FileName),
                DocumentUrl = fileKey,
                DocumentType = request.File.ContentType,
                SizeInBytes = request.File.Length,
                // Legacy column name; currently represents the authenticated upload owner.
                UploadedByUserId = _currentUser.UserId,
                ClientRequestId = null,
                CaseId = null,
                CreatedAt = DateTime.UtcNow
            };

            _db.CaseDocuments.Add(document);
            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                await _storageService.DeleteFileAsync(fileKey);
                throw;
            }

            return Result<UploadDocumentResponseDto>.Success(new UploadDocumentResponseDto
            {
                DocumentId = document.Id,
                DocumentName = document.DocumentName,
                Type = document.DocumentType,
                SizeInBytes= document.SizeInBytes,
                CreatedAt = document.CreatedAt

            });
        }
    }
}
