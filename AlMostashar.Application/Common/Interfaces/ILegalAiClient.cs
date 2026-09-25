using AlMostashar.Application.Features.LegalAi.DTOs;

namespace AlMostashar.Application.Common.Interfaces;

public interface ILegalAiClient
{
    Task<LegalAiChatResponse> AskAsync(string query, CancellationToken cancellationToken);

    Task WarmupAsync(CancellationToken cancellationToken);

    Task<LegalAiInfoResponse> GetInfoAsync(CancellationToken cancellationToken);

    Task<LegalAiHealthResponse> HealthAsync(CancellationToken cancellationToken);
}
