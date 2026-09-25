namespace AlMostashar.Application.Features.ClientRequests.DTOs;

public class DocumentDto
{
    public int Id { get; set; }
    public string DocumentName { get; set; } = null!;
    public string Type { get; set; } = null!;
    public long SizeInBytes { get; set; }
    public DateTime CreatedAt { get; set; }
}
