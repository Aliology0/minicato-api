namespace AlMostashar.Infrastructure.Options;

public sealed class LegalAiOptions
{
    public const string SectionName = "LegalAi";

    public string BaseUrl { get; set; } = "https://loaywael10-al-mostashar-legal-rag.hf.space";

    public string? ApiKey { get; set; }

    public int TimeoutSeconds { get; set; } = 120;

    public string HeaderName { get; set; } = "X-Internal-Service-Token";

    public bool WarmupOnStartup { get; set; } = false;

    public int WarmupDelaySeconds { get; set; } = 5;

    public bool CacheEnabled { get; set; } = true;

    public int CacheDurationMinutes { get; set; } = 30;
}
