namespace AlMostashar.Application.Features.ClientHome.DTOs;

public class ClientHomeAnalyticsDto
{
    public int TotalRequests { get; set; }
    public int ActiveCasesCount { get; set; }
    public int CompletedCasesCount { get; set; }
    public int PendingOffersCount { get; set; }
    public int UnpaidInvoicesCount { get; set; }
    public int UnreadMessagesCount { get; set; }
}
