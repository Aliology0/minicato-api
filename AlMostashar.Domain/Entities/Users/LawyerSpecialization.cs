namespace AlMostashar.Domain.Entities;

public class LawyerSpecialization : BaseEntity
{
    public string Title { get; set; } = null!;
    public string ArabicTitle { get; set; } = null!;

    public ICollection<Lawyer>? Lawyers { get; set; }
}
