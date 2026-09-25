namespace AlMostashar.Application.Features.LegalService.DTOs
{
    public record LegalServiceResponse(
        int Id,
        string Title,
        string Summary,
        string FullDescription,
        string ServiceType,
        string? IconUrl,
        string? RequiredDocuments,
        string? ExpectedDuration,
        bool IsActive,
        DateTime CreatedAt);
}
