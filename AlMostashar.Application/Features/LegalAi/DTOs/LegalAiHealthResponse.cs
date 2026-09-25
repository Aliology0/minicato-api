using System.Text.Json.Serialization;

namespace AlMostashar.Application.Features.LegalAi.DTOs;

public sealed class LegalAiHealthResponse
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("service")]
    public string? Service { get; set; }
}
