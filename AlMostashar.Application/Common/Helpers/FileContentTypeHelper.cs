using AlMostashar.Application.Common.Enums;

namespace AlMostashar.Application.Common.Helpers
{
    public static class FileContentTypeHelper
    {
        /// <summary>
        /// Maps a file extension (e.g. ".pdf", ".jpg") to the corresponding <see cref="FileContentType"/>.
        /// Returns null if the extension is not supported.
        /// </summary>
        public static FileContentType? FromExtension(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".pdf"  => FileContentType.Pdf,
                ".jpg"  => FileContentType.Jpeg,
                ".jpeg" => FileContentType.Jpeg,
                ".png"  => FileContentType.Png,
                ".webp" => FileContentType.Webp,
                ".heic" => FileContentType.Heic,
                ".doc"  => FileContentType.Doc,
                ".docx" => FileContentType.Docx,
                ".xls"  => FileContentType.Xls,
                ".xlsx" => FileContentType.Xlsx,
                ".csv"  => FileContentType.Csv,
                ".txt"  => FileContentType.Txt,
                _       => null
            };
        }

        /// <summary>
        /// The set of allowed file extensions for identity document uploads.
        /// </summary>
        public static readonly string[] AllowedExtensions =
            { ".pdf", ".jpg", ".jpeg", ".png", ".webp", ".heic", ".doc", ".docx", ".xls", ".xlsx", ".csv", ".txt" };

        /// <summary>
        /// The set of allowed file extensions for image uploads (e.g. profile pictures).
        /// </summary>
        public static readonly string[] AllowedImageExtensions =
            { ".jpg", ".jpeg", ".png", ".webp", ".heic" };

        /// <summary>
        /// Allowed extensions for identity / verification documents.
        /// </summary>
        public static readonly string[] AllowedIdentityDocumentExtensions =
            { ".pdf", ".jpg", ".jpeg", ".png", ".webp", ".heic" };

        public static bool HasAllowedExtension(Microsoft.AspNetCore.Http.IFormFile file, IReadOnlyCollection<string> allowedExtensions)
        {
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            return !string.IsNullOrEmpty(extension) && allowedExtensions.Contains(extension);
        }

        public static bool IsNonEmpty(Microsoft.AspNetCore.Http.IFormFile file)
            => file.Length > 0;

        public static bool IsWithinSize(Microsoft.AspNetCore.Http.IFormFile file, long maxFileSizeBytes)
            => file.Length > 0 && file.Length <= maxFileSizeBytes;

        public static bool HasMatchingContentType(Microsoft.AspNetCore.Http.IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            var contentType = file.ContentType?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(contentType))
                return false;

            return extension switch
            {
                ".pdf" => contentType == "application/pdf",
                ".jpg" or ".jpeg" => contentType is "image/jpeg" or "image/jpg",
                ".png" => contentType == "image/png",
                ".webp" => contentType == "image/webp",
                ".heic" => contentType is "image/heic" or "image/heif",
                ".doc" => contentType == "application/msword",
                ".docx" => contentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => contentType == "application/ms-excel" || contentType == "application/vnd.ms-excel",
                ".xlsx" => contentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".csv" => contentType is "text/csv" or "application/vnd.ms-excel",
                ".txt" => contentType == "text/plain",
                _ => false
            };
        }
    }
}
