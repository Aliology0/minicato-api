using System.Text.Json.Serialization;

namespace AlMostashar.Application.Features.LegalAi.DTOs;

public sealed class LegalAiLlmDto
{
    [JsonPropertyName("called")]
    public bool Called { get; set; }

    [JsonPropertyName("succeeded")]
    public bool Succeeded { get; set; }

    [JsonPropertyName("provider")]
    public string? Provider { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }
}
