using System.Text.Json.Serialization;

namespace AlMostashar.Application.Features.LegalAi.DTOs;

public sealed class LegalAiChatRequest
{
    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;
}
