namespace AlMostashar.Application.Common.Enums
{
    public enum FileContentType
    {
        Pdf = 0,
        Jpeg = 1,
        Jpg = 2,
        Png = 3,
        Webp = 4,
        Docx = 5,
        Xlsx = 6,
        Txt = 7,
        Heic = 8,
        Doc = 9,
        Xls = 10,
        Csv = 11
    }

    public static class FileContentTypeExtensions
    {
        public static string ToMimeType(this FileContentType type) => type switch
        {
            FileContentType.Pdf  => "application/pdf",
            FileContentType.Jpeg => "image/jpeg",
            FileContentType.Jpg => "image/jpg",
            FileContentType.Png  => "image/png",
            FileContentType.Webp => "image/webp",
            FileContentType.Docx => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            FileContentType.Doc  => "application/msword",
            FileContentType.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileContentType.Xls  => "application/vnd.ms-excel",
            FileContentType.Csv  => "text/csv",
            FileContentType.Txt  => "text/plain",
            FileContentType.Heic => "image/heic",
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unsupported content type: {type}")
        };
    }
}
