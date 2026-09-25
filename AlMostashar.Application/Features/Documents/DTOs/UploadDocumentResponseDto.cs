namespace AlMostashar.Application.Features.Documents.DTOs
{
    public class UploadDocumentResponseDto
    {
        public int DocumentId { get; set; }
        public string DocumentName { get; set; } = null!;
        public string Type { get; set; } = null!;
        public long SizeInBytes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
