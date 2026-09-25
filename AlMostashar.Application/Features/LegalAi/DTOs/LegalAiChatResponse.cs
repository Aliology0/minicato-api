using System.Text.Json.Serialization;

namespace AlMostashar.Application.Features.LegalAi.DTOs;

public sealed class LegalAiChatResponse
{
    [JsonPropertyName("answer_mode")]
    public string AnswerMode { get; set; } = string.Empty;

    [JsonPropertyName("final_answer")]
    public string FinalAnswer { get; set; } = string.Empty;

    [JsonPropertyName("warning")]
    public string? Warning { get; set; }

    [JsonPropertyName("is_legal_question")]
    public bool IsLegalQuestion { get; set; }

    [JsonPropertyName("is_supported_by_internal_sources")]
    public bool IsSupportedByInternalSources { get; set; }

    [JsonPropertyName("is_out_of_internal_corpus")]
    public bool IsOutOfInternalCorpus { get; set; }

    [JsonPropertyName("sources")]
    public List<LegalAiSourceDto> Sources { get; set; } = [];

    [JsonPropertyName("llm")]
    public LegalAiLlmDto Llm { get; set; } = new();

    [JsonPropertyName("answer_parts")]
    public LegalAiAnswerPartsDto? AnswerParts { get; set; }

    [JsonIgnore]
    public bool CacheHit { get; set; }

    [JsonIgnore]
    public long ElapsedMilliseconds { get; set; }
}
