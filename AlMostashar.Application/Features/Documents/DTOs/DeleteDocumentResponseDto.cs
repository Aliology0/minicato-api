namespace AlMostashar.Application.Features.Documents.DTOs
{
    public class DeleteDocumentResponseDto
    {
        public int DocumentId { get; set; }
        public bool Deleted { get; set; }
        public string Message { get; set; } = null!;
    }
}
