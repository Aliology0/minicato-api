namespace AlMostashar.Application.Features.Cases.DTOs;

public class CaseDocumentDto
{
    public int Id { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
