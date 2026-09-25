namespace AlMostashar.Infrastructure.Options;

public sealed class DocumentCleanupOptions
{
    public const string SectionName = "DocumentCleanup";
    public int RetentionHours { get; set; } = 48;
    public int IntervalHours { get; set; } = 6;
}
