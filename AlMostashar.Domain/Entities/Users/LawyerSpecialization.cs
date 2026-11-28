namespace AlMostashar.Domain.Entities;

public class LawyerSpecialization : BaseEntity
{
    public string Title { get; set; } = null!;
    public string ArabicTitle { get; set; } = null!;
    public bool IsActive { get; set; } = true;

    public ICollection<Lawyer>? Lawyers { get; set; }
}
