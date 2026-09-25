namespace AlMostashar.Application.Features.ClientRequests.DTOs;

public class AcceptOfferResponseDto
{
    public string Message { get; set; } = null!;
    public int InvoiceId { get; set; }
    public int ClientRequestId { get; set; }
}
