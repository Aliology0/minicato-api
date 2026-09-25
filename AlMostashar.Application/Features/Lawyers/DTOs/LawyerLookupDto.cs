namespace AlMostashar.Application.Features.Lawyers.DTOs
{
    public class LawyerLookupDto
    {
        public int LawyerId { get; set; }
        public string FullName { get; set; } = null!;
        public string ProfileImage { get; set; } = null!;
        public double Rating { get; set; }
        public bool IsActive { get; set; }
    }
}
