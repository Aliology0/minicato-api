using System.Text.Json.Serialization;

namespace AlMostashar.Application.Features.LegalAi.DTOs;

public sealed class LegalAiAnswerPartsDto
{
    [JsonPropertyName("intro")]
    public string? Intro { get; set; }

    [JsonPropertyName("section_title")]
    public string? SectionTitle { get; set; }

    [JsonPropertyName("bullets")]
    public List<string> Bullets { get; set; } = [];

    [JsonPropertyName("legal_basis")]
    public string? LegalBasis { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }
}
