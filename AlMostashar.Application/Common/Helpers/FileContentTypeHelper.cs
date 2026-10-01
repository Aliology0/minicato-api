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
    }
}
