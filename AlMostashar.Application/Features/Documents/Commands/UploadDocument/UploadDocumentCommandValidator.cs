using AlMostashar.Application.Common.Constants;
using AlMostashar.Application.Common.Helpers;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace AlMostashar.Application.Features.Documents.Commands.UploadDocument
{
    public class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
    {
        private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB

        public UploadDocumentCommandValidator()
        {
            RuleFor(x => x.File)
                .NotNull().WithMessage(Messages.Validation.FileRequired)
                .Must(BeValidSize!).WithMessage(Messages.Validation.FileTooLarge)
                .Must(BeAllowedExtension!).WithMessage(Messages.Validation.FileTypeNotAllowed)
                .Must(HaveMatchingContentType!).WithMessage(Messages.Validation.FileTypeNotAllowed);
        }

        private static bool BeValidSize(IFormFile file) => file.Length <= MaxFileSizeBytes;

        private static bool BeAllowedExtension(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            return !string.IsNullOrEmpty(extension)
                && FileContentTypeHelper.AllowedExtensions.Contains(extension);
        }

        private static bool HaveMatchingContentType(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            return extension switch
            {
                ".pdf" => file.ContentType == "application/pdf",
                ".jpg" or ".jpeg" => file.ContentType == "image/jpeg",
                ".png" => file.ContentType == "image/png",
                ".webp" => file.ContentType == "image/webp",
                ".heic" => file.ContentType is "image/heic" or "image/heif",
                ".doc" => file.ContentType == "application/msword",
                ".docx" => file.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => file.ContentType == "application/vnd.ms-excel",
                ".xlsx" => file.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".csv" => file.ContentType is "text/csv" or "application/vnd.ms-excel",
                ".txt" => file.ContentType == "text/plain",
                _ => false
            };
        }
    }
}
