using System.Text.Json.Serialization;

namespace AlMostashar.Application.Features.LegalAi.DTOs;

public sealed class LegalAiInfoResponse
{
    [JsonPropertyName("service")]
    public string? Service { get; set; }

    [JsonPropertyName("app_name")]
    public string? AppName { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("llm_provider")]
    public string? LlmProvider { get; set; }

    [JsonPropertyName("retrieval_backend")]
    public string? RetrievalBackend { get; set; }

    [JsonPropertyName("embedding_backend")]
    public string? EmbeddingBackend { get; set; }

    [JsonPropertyName("answer_modes")]
    public List<string> AnswerModes { get; set; } = [];

    [JsonPropertyName("supported_internal_domains")]
    public List<string> SupportedInternalDomains { get; set; } = [];

    [JsonPropertyName("out_of_internal_corpus_examples")]
    public List<string> OutOfInternalCorpusExamples { get; set; } = [];
}
