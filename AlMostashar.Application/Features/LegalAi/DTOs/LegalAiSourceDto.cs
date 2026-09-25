using System.Text.Json.Serialization;

namespace AlMostashar.Application.Features.LegalAi.DTOs;

public sealed class LegalAiSourceDto
{
    [JsonPropertyName("law_name")]
    public string? LawName { get; set; }

    [JsonPropertyName("article_number")]
    public string? ArticleNumber { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("source_url")]
    public string? SourceUrl { get; set; }

    [JsonPropertyName("legal_domain")]
    public string? LegalDomain { get; set; }
}
