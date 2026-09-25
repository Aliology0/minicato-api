namespace AlMostashar.Domain.Entities
{
    public class Admin : User
    {
        // Navigation — Step 4: 1:N (Admin → Lawyer via Verifies)
        public ICollection<Lawyer> VerifiedLawyers { get; set; } = new List<Lawyer>();

        // Navigation — 1:N (Admin → Reports Reviewed)
        public ICollection<Report> ReviewedReports { get; set; } = new List<Report>();

        // Note: Admin → Case (Manages) relationship excluded per user request
    }
}
