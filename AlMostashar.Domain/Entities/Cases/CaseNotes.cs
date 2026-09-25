namespace AlMostashar.Domain.Entities;

public class CaseNotes : BaseEntity
{
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CaseId { get; set; }
    public Case Case { get; set; }
}
