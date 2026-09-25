namespace AlMostashar.Application.Features.LawyerService.DTOs
{
public record LawyerServiceResponse(int ServiceId, string Title, string Description, decimal Price, string Duration, bool IsActive);
}
