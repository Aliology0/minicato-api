namespace AlMostashar.Application.Features.Documents.DTOs
{
    public class GetPresignedUrlResponseDto
    {
        public int DocumentId { get; set; }
        public string DocumentName { get; set; } = null!;
        public string Url { get; set; } = null!;
        public string Type { get; set; } = null!;
        public long SizeInBytes { get; set; }

        public int ExpirationMinutes { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
