namespace AlMostashar.Application.Features.Cases.DTOs;

public class CaseNoteDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
