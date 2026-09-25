namespace AlMostashar.Domain.Entities;

public class CaseTimeline : BaseEntity
{
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CaseId { get; set; }
    public Case Case { get; set; }
}
