namespace AlMostashar.Application.Features.Lawyers.DTOs;

public class LawyerAnalyticsDto
{
    public double AverageRating { get; set; }
    public int NewIncomingRequestsCount { get; set; }
    public decimal MonthlyEarnings { get; set; }
    public int OpenCasesCount { get; set; }
    public int CompletedCasesCount { get; set; }
}
