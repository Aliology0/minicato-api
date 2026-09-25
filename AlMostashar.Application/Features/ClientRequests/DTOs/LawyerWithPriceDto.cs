namespace AlMostashar.Application.Features.ClientRequests.DTOs;

public class LawyerWithPriceDto
{
    public int LawyerId { get; set; }
    public string FullName { get; set; } = null!;
    public string? ProfileImage { get; set; }
    public string? Specialization { get; set; }
    public decimal Price { get; set; }
    public string Duration { get; set; } = null!;
}
